using UnityEngine;

public class SpellController : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f; // tự huỷ sau 3 giây

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // tự hủy sau `lifetime` giây
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player trúng đạn!");
            // collision.GetComponent<PlayerControl>()?.TakeDamage(damage); // nếu có
            Destroy(gameObject);
        }
        else if (!collision.isTrigger)
        {
            Destroy(gameObject); // nếu đụng vào vật khác
        }
    }
}
