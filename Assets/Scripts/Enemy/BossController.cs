using System;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    private Vector3 originalScale;

    [SerializeField] protected float enemyMoveSpeed = 1f;
    [SerializeField] protected PlayerControl player;
    [SerializeField] protected int maxHp = 50;
    protected float currentHp;
    [SerializeField] private Image hpBar;
    [SerializeField] protected int enterDamage = 10;
    [SerializeField] protected int stayDamage = 1;

    private Rigidbody2D rb;
    private Animator animator;

    public float meleeRange = 2f;
    public float rangedRange = 5f;

    public float meleeAttackCooldown = 1.5f;
    public float rangedAttackCooldown = 5f;

    //private float lastMeleeAttackTime;
    private float lastRangedAttackTime;

    public GameObject rangedAttackPrefab;
    public Transform firePoint;

    public void Awake()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = 1f; // Initialize HP bar to full
        }
    }

    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerControl>();
        originalScale = transform.localScale;
        currentHp = maxHp;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        MoveToPlayer();
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        float currentTime = Time.time;

        // Boss nhìn về hướng người chơi
        Vector3 scale = transform.localScale;
        if (player.transform.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x); // Quay trái
        else
            scale.x = Mathf.Abs(scale.x);  // Quay phải
        transform.localScale = scale;

        // Tấn công theo khoảng cách và cooldown riêng
        //if (distanceToPlayer <= meleeRange && currentTime - lastMeleeAttackTime >= meleeAttackCooldown)
        //{
        //    MeleeAttack();
        //    lastMeleeAttackTime = currentTime;
        //}
        if (distanceToPlayer <= rangedRange && distanceToPlayer > meleeRange && currentTime - lastRangedAttackTime >= rangedAttackCooldown)
        {
            RangedAttack();
            lastRangedAttackTime = currentTime;
            
        }
    }

    //void MeleeAttack()
    //{
    //    Debug.Log("Boss chém người chơi!");
    //    animator.SetTrigger("IsAttack"); // Giả sử bạn có animation chém
    //    // TODO: Gây sát thương thật sự ở đây
    //}

    void RangedAttack()
    {

        Vector2 direction = (player.transform.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(rangedAttackPrefab, firePoint.position, Quaternion.identity);

        SpellController ranged = projectile.GetComponent<SpellController>();
        if (ranged != null)
        {
            ranged.SetDirection(direction);
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

    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log("Enemy died.");
        animator.SetTrigger("IsDie");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                animator.SetBool("IsAttack", true);
                //player.TakeDamage(enterDamage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                //player.TakeDamage(stayDamage);
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
            hpBar.fillAmount = currentHp / maxHp;
        }
    }

    // Vẽ phạm vi tấn công
    private void OnDrawGizmosSelected()
    {
       

        // Vẽ phạm vi tung chưởng (xanh)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}
