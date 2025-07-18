using UnityEngine;

public class SkillBehaviour : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 1.5f;
    private Vector2 moveDirection;
    private float damage;

    public GameObject hitEffectPrefab; // Prefab hiệu ứng nổ

    public void Initialize(Vector2 direction, float skillDamage = 10f)
    {
        moveDirection = direction.normalized;
        damage = skillDamage;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = direction.x < 0;
        }
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Gây damage
            if (collision.GetComponent<EnemyFollow>() != null)
                collision.GetComponent<EnemyFollow>().TakeHit(transform.position, 3f, damage);
            else if (collision.GetComponent<BossController>() != null)
                collision.GetComponent<BossController>().TakeDamage((int)damage);

            // Tạo hiệu ứng nổ tại vị trí va chạm
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}

