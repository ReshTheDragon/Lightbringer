using System;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    private Vector3 originalScale;

    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected PlayerControl player;
    [SerializeField] protected int maxHp = 500;
    protected int currentHp; // Đổi từ float sang int để khớp với maxHp
    [SerializeField] private Image hpBar;
    [SerializeField] protected int enterDamage = 10;
    [SerializeField] protected int stayDamage = 1;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Ranged Attack Settings")]
    public float rangedRange = 5f;
    public float rangedAttackCooldown = 5f;
    public int rangedAttackDamage = 15;
    private float lastRangedAttackTime;
    public GameObject rangedAttackPrefab;
    public Transform firePoint;

    [Header("Spell Cooldown")]
    public float spellCooldown = 2f;
    private float lastSpellTime;
    private bool isSpellOnCooldown = false;

    [Header("Cast Animation")]
    public float castDuration = 1.5f;
    private bool isCasting = false;

    public void Awake()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = 1f;
        }
    }

    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
        originalScale = transform.localScale;
        currentHp = maxHp;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Debug để kiểm tra
        Debug.Log($"Boss initialized with HP: {currentHp}/{maxHp}");
    }

    protected virtual void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float currentTime = Time.time;

        if (!isCasting)
        {
            Vector3 scale = transform.localScale;
            if (player.transform.position.x < transform.position.x)
                scale.x = -Mathf.Abs(scale.x);
            else
                scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (isCasting && currentTime - lastSpellTime >= castDuration)
        {
            isCasting = false;
            animator.SetBool("IsCast", false);
        }

        if (isSpellOnCooldown && currentTime - lastSpellTime >= spellCooldown)
        {
            isSpellOnCooldown = false;
        }

        if (!isCasting)
        {
            MoveToPlayer();
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }

        if (distanceToPlayer > rangedRange &&
            !isSpellOnCooldown &&
            !isCasting &&
            currentTime - lastRangedAttackTime >= rangedAttackCooldown)
        {
            StartCasting();
            lastRangedAttackTime = currentTime;
        }
    }

    void StartCasting()
    {
        isCasting = true;
        animator.SetBool("IsCast", true);
        animator.SetBool("IsWalking", false);
        lastSpellTime = Time.time;
        isSpellOnCooldown = true;

        Invoke("RangedAttack", 0.5f);
    }

    void RangedAttack()
    {
        Vector3 spellSpawnPosition = new Vector3(player.transform.position.x, player.transform.position.y + 3f, player.transform.position.z);
        GameObject projectile = Instantiate(rangedAttackPrefab, spellSpawnPosition, Quaternion.identity);

        SpellController ranged = projectile.GetComponent<SpellController>();
        if (ranged != null)
        {
            Vector2 downDirection = Vector2.down;
            ranged.SetDirection(downDirection);
            ranged.SetDamage(rangedAttackDamage);
        }
    }

    protected void MoveToPlayer()
    {
        if (player != null)
        {
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemyMoveSpeed * Time.deltaTime);
            FlipEnemy();
        }
        else
        {
            Debug.LogWarning("Player is NULL — cannot move.");
        }
    }

    protected void FlipEnemy()
    {
        if (player != null)
        {
            float direction = player.transform.position.x > transform.position.x ? 1f : -1f;
            transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
        }
    }

    // Phương thức TakeDamage cũ - chỉ nhận damage
    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateHpBar();

        Debug.Log($"Boss took {damage} damage. Current HP: {currentHp}/{maxHp}");
        if (currentHp <= 0)
        {
            animator.SetTrigger("IsDie");
            Die();
        }
    }

    // Phương thức TakeDamage mới - nhận thêm knockback (để tương thích với code attack của bạn)
    public virtual void TakeDamage(Vector2 attackerPosition, float knockbackForce, int damage)
    {
        // Gọi phương thức TakeDamage cũ
        TakeDamage(damage);

        // Thêm knockback nếu cần
        if (knockbackForce > 0)
        {
            Vector2 knockbackDirection = (transform.position - (Vector3)attackerPosition).normalized;
            if (rb != null)
            {
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    public virtual void Die()
    {
        Debug.Log("Boss died.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                animator.SetBool("IsAttack", true);
                player.TakeDamage(enterDamage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                player.TakeDamage(stayDamage);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("IsAttack", false);
        }
    }

    private void UpdateHpBar()
    {
        if (hpBar != null)
        {
            // Ép kiểu để tránh lỗi chia số
            hpBar.fillAmount = (float)currentHp / (float)maxHp;
            Debug.Log($"HP Bar updated: {hpBar.fillAmount} (HP: {currentHp}/{maxHp})");
        }
        else
        {
            Debug.LogWarning("HP Bar is null!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}