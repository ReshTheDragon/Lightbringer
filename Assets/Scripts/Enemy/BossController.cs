using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    private Vector3 originalScale;
    private Rigidbody2D rb;
    private Animator animator;

    [SerializeField] protected PlayerControl player;
    [SerializeField] private Image hpBar;

    [Header("Stats")]
    [SerializeField] private int maxHp = 500;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int enterDamage = 10;
    [SerializeField] private int stayDamage = 1;

    private int currentHp;

    [Header("Ranged Attack")]
    public float rangedRange = 5f;
    public float rangedAttackCooldown = 5f;
    public int rangedAttackDamage = 15;
    public GameObject rangedAttackPrefab;
    public float spellCastTime = 1.5f;

    private bool isCasting = false;
    private bool canCast = true;

    private void Awake()
    {
        if (hpBar != null) hpBar.fillAmount = 1f;
        currentHp = maxHp;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = FindAnyObjectByType<PlayerControl>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        FlipTowardPlayer();

        if (!isCasting)
        {
            if (distanceToPlayer > rangedRange && canCast)
            {
                StartCoroutine(CastSpell());
            }
            else
            {
                MoveToPlayer();
            }
        }
    }

    private void MoveToPlayer()
    {
        if (player == null) return;

        animator.SetBool("IsWalking", true);
        Vector2 target = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
        rb.MovePosition(target);
    }

    private void FlipTowardPlayer()
    {
        if (player == null) return;
        float direction = player.transform.position.x > transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
    }

    private IEnumerator CastSpell()
    {
        isCasting = true;
        canCast = false;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsCast", true);

        yield return new WaitForSeconds(spellCastTime);

        animator.SetBool("IsCast", false);
        SpawnSpell();

        yield return new WaitForSeconds(rangedAttackCooldown);
        canCast = true;
        isCasting = false;
    }

    private void SpawnSpell()
    {
        if (player == null) return;

        Vector3 spawnPos = new Vector3(player.transform.position.x, player.transform.position.y + 3f, 0);
        GameObject spell = Instantiate(rangedAttackPrefab, spawnPos, Quaternion.identity);

        SpellController sc = spell.GetComponent<SpellController>();
        if (sc != null)
        {
            sc.SetDirection(Vector2.down);
            sc.SetDamage(rangedAttackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateHpBar();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(Vector2 attackerPos, float knockbackForce, int damage)
    {
        TakeDamage(damage);

        if (knockbackForce > 0 && rb != null)
        {
            Vector2 knockDir = ((Vector2)transform.position - attackerPos).normalized;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = (float)currentHp / maxHp;
        }
    }

    private void Die()
    {
        animator.SetTrigger("IsDie");
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player != null)
        {
            animator.SetBool("IsAttack", true);
            player.TakeDamage(enterDamage);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && player != null)
        {
            player.TakeDamage(stayDamage);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("IsAttack", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}
