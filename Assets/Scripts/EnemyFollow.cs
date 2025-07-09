using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float chaseRadius = 5f;

    public GameObject healthBarPrefab; // Prefab thanh máu

    private bool isCollidingWithPlayer = false;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isDying = false;
    private float maxHealth = 100f;
    private float currentHealth;

    private GameObject healthBarInstance;
    private Image healthFill;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Khởi tạo máu
        currentHealth = maxHealth;

        // Tạo thanh máu và gán vào enemy
        healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
        healthBarInstance.transform.SetParent(transform);

        // Tìm image fill trong health bar
        healthFill = healthBarInstance.transform.Find("BackGround/Fill").GetComponent<Image>();
    }

    void Update()
    {
        if (player == null || isCollidingWithPlayer || isDying) return;

        // Tính khoảng cách và di chuyển enemy
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < chaseRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        // Cập nhật vị trí thanh máu theo enemy
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + Vector3.up * 1.2f;
        }
    }

    public void TakeHit(Vector3 attackerPos, float force, float damage)
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


    IEnumerator FlashAndDie()
    {
        isDying = true;
        float flashDuration = 0.5f;
        float flashSpeed = 0.1f;
        float elapsedTime = 0f;

        // Nhấp nháy trước khi chết
        while (elapsedTime < flashDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashSpeed);
            elapsedTime += flashSpeed;
        }

        Destroy(healthBarInstance); // Xóa thanh máu
        Destroy(gameObject);        // Xóa enemy
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            // Mất 20% máu
            float damage = maxHealth * 0.2f;
            TakeHit(collision.transform.position, 0f, damage);  // 0 force vì chưởng không đẩy lùi

            Destroy(collision.gameObject);  // Xóa projectile sau khi chạm
        }
    }

}
