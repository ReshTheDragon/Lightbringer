using UnityEngine;
using System.Collections;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float chaseRadius = 5f;

    private bool isCollidingWithPlayer = false;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int hitCount = 0;
    private bool isDying = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || isCollidingWithPlayer || isDying) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < chaseRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    public void TakeHit(Vector3 attackerPos, float force)
    {
        if (isDying) return;

        hitCount++;
        Debug.Log("Enemy hit! Count: " + hitCount);

        // Tính vector đẩy lùi
        Vector2 knockbackDir = (transform.position - attackerPos).normalized;
        rb.AddForce(knockbackDir * force, ForceMode2D.Impulse);

        if (hitCount >= 4)
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

        while (elapsedTime < flashDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashSpeed);
            elapsedTime += flashSpeed;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
