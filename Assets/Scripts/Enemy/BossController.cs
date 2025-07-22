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
    [SerializeField] private int maxHp = 10;
    [SerializeField] private float moveSpeed = 10f;
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

    [Header("Summon")]
    public GameObject summonEnemyPrefab;
    public int summonCount = 3;
    public float summonRadius = 2f;

    private bool hasSummoned = false;
    public float summonDelay = 1f;

    // Danh sách lưu trữ các minion đã summon
    private System.Collections.Generic.List<GameObject> summonedMinions = new System.Collections.Generic.List<GameObject>();

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

        // Chỉ di chuyển và cast khi KHÔNG đang cast
        if (!isCasting)
        {
            if (distanceToPlayer > rangedRange && canCast)
            {
                StartCoroutine(CastSpell());
            }
            else
            {
                // Chỉ di chuyển khi không cast spell
                MoveToPlayer();
            }
        }
        else
        {
            // Khi đang cast, dừng animation walking
            animator.SetBool("IsWalking", false);
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

        // Kiểm tra summon quái nếu chưa summon và máu <= 50%
        if (!hasSummoned && currentHp <= maxHp / 2)
        {
            SummonEnemies();
            hasSummoned = true;
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(Vector2 attackerPos, float knockbackForce, int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UpdateHpBar();

        // Kiểm tra summon quái nếu chưa summon và máu <= 50%
        if (!hasSummoned && currentHp <= maxHp / 2)
        {
            SummonEnemies();
            hasSummoned = true;
        }

        if (knockbackForce > 0 && rb != null)
        {
            Vector2 knockDir = ((Vector2)transform.position - attackerPos).normalized;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHp <= 0)
        {
            Die();
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
        
        rb.linearVelocity = Vector2.zero;
        this.enabled = false; 

        
        animator.SetTrigger("IsDie");
       

        

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log("Boss is dying...");

        KillAllMinions();

      
        yield return new WaitForSeconds(1f);
        PlayerPrefs.SetInt("IsWin", 1);

        Debug.Log("Loading ending scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndingScene");

       
        Destroy(gameObject);
    }

    private void KillAllMinions()
    {
        Debug.Log("Killing all minions...");

        // Tìm tất cả EnemyFollow trong scene
        EnemyFollow[] allMinions = FindObjectsOfType<EnemyFollow>();
        foreach (var minion in allMinions)
        {
            if (minion != null)
            {
                // Gọi method Die() - method này sẽ destroy minion sau 1s
                minion.Die();
            }
        }

        Debug.Log($"Killed {allMinions.Length} minions");
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

        // Hiển thị summon radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }

    private void SummonEnemies()
    {
        StartCoroutine(SummonEnemiesCoroutine());
    }

    private IEnumerator SummonEnemiesCoroutine()
    {
        Debug.Log("Boss is summoning minions at 50% health!");

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * summonRadius;
            GameObject enemy = Instantiate(summonEnemyPrefab, spawnPos, Quaternion.identity);

            // Thêm minion vào danh sách để quản lý
            summonedMinions.Add(enemy);

            EnemyFollow enemyFollow = enemy.GetComponent<EnemyFollow>();
            if (enemyFollow != null)
            {
                enemyFollow.player = player.transform;
            }

            yield return new WaitForSeconds(summonDelay);
        }

        Debug.Log($"Boss summoned {summonCount} minions!");
    }
}