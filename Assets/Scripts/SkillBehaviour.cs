using UnityEngine;

public class SkillBehaviour : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 1.5f;
    private Vector2 moveDirection;
    private float damage;

    public GameObject hitEffectPrefab;

    public void Initialize(Vector2 direction, float skillDamage = 10f)
    {
        moveDirection = direction.normalized;
        damage = skillDamage;

        // Xoay theo hướng di chuyển
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.GetComponent<EnemyFollow>() != null)
                collision.GetComponent<EnemyFollow>().TakeHit(transform.position, 3f, damage);
            else if (collision.GetComponent<BossController>() != null)
                collision.GetComponent<BossController>().TakeDamage((int)damage);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
