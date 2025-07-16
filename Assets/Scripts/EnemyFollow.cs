using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float chaseRadius = 5f;
    public float attackDamage = 10f; // Damage dealt per hit
    public float attackCooldown = 1f; // Cooldown between attacks
    private float lastAttackTime; // Time when the last attack occurred

    public GameObject healthBarPrefab; // Prefab thanh máu

    private bool isCollidingWithPlayer = false;
    protected bool isPlayerInAttackRange = false; // Changed to protected
    protected Animator animator; // Changed to protected
    protected Rigidbody2D rb; // Changed to protected
    protected SpriteRenderer spriteRenderer; // Changed to protected

    protected bool isDying = false; // Changed to protected
    protected float maxHealth = 100f; // Changed to protected
    protected float currentHealth; // Changed to protected

    private GameObject healthBarInstance;
    private Image healthFill;

    protected virtual void Start() // Made virtual
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Khởi tạo máu
        currentHealth = maxHealth;

        lastAttackTime = -attackCooldown; // Allow immediate attack

        // Tạo thanh máu và gán vào enemy
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
        healthBarInstance.transform.SetParent(transform);

        // Tìm image fill trong health bar
        healthFill = healthBarInstance.transform.Find("BackGround/Fill").GetComponent<Image>();
    }

    protected virtual void Update() // Made virtual
    {
        if (player == null || isDying) return;

        // Tính khoảng cách và di chuyển enemy
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < chaseRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition((Vector2)transform.position + (direction * speed * Time.deltaTime));

            // Flip enemy theo hướng di chuyển
            if (direction.x > 0)
            {
                spriteRenderer.flipX = false; // Quay mặt phải
            }
            else if (direction.x < 0)
            {
                spriteRenderer.flipX = true; // Quay mặt trái
            }
        }

        // Cập nhật vị trí thanh máu theo enemy
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + Vector3.up * 1.2f;
        }

        // Attack player if in range and cooldown is over
        if (isPlayerInAttackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }

    protected virtual void AttackPlayer() // Made virtual
    {
        PlayerControl playerControl = player.GetComponent<PlayerControl>();
        if (playerControl != null)
        {

            playerControl.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
            Debug.Log($"Enemy attacked player for {attackDamage} damage.");
        }
    }

    public virtual void TakeHit(Vector3 attackerPos, float force, float damage) // Made virtual
    {
        if (isDying) return;

        Debug.Log("Enemy hit!");

        // Tính vector đẩy lùi
        Vector2 knockbackDir = (transform.position - attackerPos).normalized;
        rb.AddForce(knockbackDir * force, ForceMode2D.Impulse);

        // Trừ máu
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Update fill amount
        healthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(FlashAndDie());
        }
    }

    protected virtual IEnumerator FlashAndDie() // Made virtual
    {
        isDying = true;
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        animator.SetTrigger("IsDead");

        // Chờ animation chết chạy xong (hoặc đơn giản delay 1 giây)
        yield return new WaitForSeconds(1f);

        Destroy(healthBarInstance);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) // Made virtual
    {
        if (collision.CompareTag("Projectile"))
        {
            // Mất 20% máu
            float damage = maxHealth * 0.2f;
            TakeHit(collision.transform.position, 0f, damage);  // 0 force vì chưởng không đẩy lùi
            Destroy(collision.gameObject);  // Xóa projectile sau khi chạm
        }
        else if (collision.CompareTag("Player") && !isDying)
        {
            isPlayerInAttackRange = true;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) // Made virtual
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInAttackRange = false;
        }
    }

    public virtual void Die() // Made virtual
    {
        if (isDying) return;
        this.enabled = false;

        isDying = true;
        StopAllCoroutines(); // Dừng mọi coroutine nếu có
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        animator.SetTrigger("IsDead");

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject, 1f); // Cho phép 1s để animation Die chạy nếu có
    }
}