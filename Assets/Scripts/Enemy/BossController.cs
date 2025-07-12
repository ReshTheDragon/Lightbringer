using System.Collections;
using Unity.VisualScripting;
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
    private bool canMove = true;

    [Header("Summon")]
    public GameObject summonEnemyPrefab;
    public int summonCount = 3;
    public float summonRadius = 2f;

    private bool hasSummoned = false;
    public float summonDelay = 1f;

    // Danh sách lưu trữ các minion đã summon
    private System.Collections.Generic.List<GameObject> summonedMinions = new System.Collections.Generic.List<GameObject>();

    private bool isDead = false;

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
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        FlipTowardPlayer();

        if (isCasting)
        {
            animator.SetBool("IsWalking", false); // boss đứng yên khi cast
            return;
        }

        MoveToPlayer(); // chỉ gọi khi không cast

        if (distanceToPlayer > rangedRange && canCast)
        {
            StartCoroutine(CastSpell());
        }

    }

    private void MoveToPlayer()
    {
        if (player == null || isDead || !canMove) return;

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
        if (isDead) yield break;
        isCasting = true;
        canCast = false;
        canMove = false;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsCast", true);

        rb.linearVelocity = Vector2.zero; // ← Chắc chắn boss đứng yên khi bắt đầu cast

        yield return new WaitForSeconds(spellCastTime);

        animator.SetBool("IsCast", false);
        SpawnSpell();

        yield return new WaitForSeconds(rangedAttackCooldown);
        canCast = true;
        isCasting = false;
        canMove = true;
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
        if (isDead) return; // Thêm kiểm tra isDead

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

    private void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = (float)currentHp / maxHp;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Die() method called");

        // Dừng mọi hoạt động
        rb.linearVelocity = Vector2.zero;
        isCasting = false;
        canCast = false;

        // Disable collider để ngăn trigger events
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Dừng tất cả animator bools
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsCast", false);
        animator.SetBool("IsAttack", false);

        Debug.Log("Setting IsDie trigger");
        animator.SetTrigger("IsDie");

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        Debug.Log("Boss is dying...");

        // Disable script để ngăn Update() chạy nhưng vẫn cho phép coroutine hoạt động
        this.enabled = false;

        // Đợi animation chết hoàn thành
        yield return new WaitForSeconds(2.5f);

        // Giết tất cả minions
        KillAllMinions();

        // Đợi thêm 1.5s để minions chết hoàn toàn
        yield return new WaitForSeconds(1.5f);

        // Load ending scene
        Debug.Log("Loading ending scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndingScene");

        // Destroy boss object cuối cùng
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
        if (isDead) return; // Thêm kiểm tra isDead

        if (collision.CompareTag("Player") && player != null)
        {
            animator.SetBool("IsAttack", true);
            player.TakeDamage(enterDamage);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead) return; // Thêm kiểm tra isDead

        if (collision.CompareTag("Player") && player != null)
        {
            player.TakeDamage(stayDamage);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isDead) return; // Thêm kiểm tra isDead

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