using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 5f;
    public float damage = 30f;
    public float lifetime = 2f;

    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    public bool isFlipped;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Tự hủy sau lifetime giây
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction, bool flip)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = flip;  // hoặc flipX tùy chiều mặc định của sprite bạn
        }
    }
    public void SetFlip(bool flip)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = flip;
        }
    }


    void Update()
    {
        // Di chuyển thẳng theo hướng
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyFollow>().TakeHit(transform.position, 5f, damage);
            Destroy(gameObject);
        }
    }
}
