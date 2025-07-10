using UnityEngine;

public class SpellController : MonoBehaviour
{
    [Header("Spell Settings")]
    public float spellSpeed = 5f;
    public float lifeTime = 5f; // Thời gian tồn tại tối đa
    public int damage = 10;

    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasExploded = false;

    [Header("Effects")]
    public GameObject explosionEffect; // Hiệu ứng nổ (optional)
    public float explosionRadius = 1f; // Bán kính nổ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tự hủy sau một khoảng thời gian
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Di chuyển spell theo hướng đã set
        if (rb != null)
        {
            rb.linearVelocity = direction * spellSpeed;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // Nếu chạm vào player
        if (other.CompareTag("Player"))
        {
            PlayerControl player = other.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            ExplodeSpell();
        }
        // Nếu chạm vào đất hoặc obstacle
        else if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            ExplodeSpell();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        // Nếu chạm vào bất kỳ thứ gì khác (đất, tường, v.v.)
        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("Obstacle"))
        {
            ExplodeSpell();
        }
    }

    void ExplodeSpell()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Tạo hiệu ứng nổ (nếu có)
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Gây damage cho player nếu trong bán kính nổ
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerControl player = collider.GetComponent<PlayerControl>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }

        // Hủy spell
        Destroy(gameObject);
    }

    // Vẽ bán kính nổ trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}