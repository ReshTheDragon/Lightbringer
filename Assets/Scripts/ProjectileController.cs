using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 5f;
    public float damage = 30f;
    public float lifetime = 2f;

    private Vector2 moveDirection;

    void Start()
    {
        // Tự hủy sau lifetime giây
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void Update()
    {
        // Di chuyển thẳng theo hướng
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Gây sát thương cho enemy
            other.GetComponent<EnemyFollow>().TakeHit(transform.position, 5f, damage);
            Destroy(gameObject); // hủy chưởng khi chạm enemy
        }
    }
}
