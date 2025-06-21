using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float attackRange = 1f;
    public float knockbackForce = 5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameUi gameUi;
    private bool isPauseMenuLoaded = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseMenuLoaded)
            {
                SceneManager.LoadScene("GamePauseMenu", LoadSceneMode.Additive);
                Time.timeScale = 0f; // Dừng game
                isPauseMenuLoaded = true;
            }
        }
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, moveY, 0).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        bool isWalking = move.magnitude > 0;
        animator.SetBool("IsWalking", isWalking);

        if (moveX > 0)
            spriteRenderer.flipX = false;
        else if (moveX < 0)
            spriteRenderer.flipX = true;

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("IsAttacking");
            Attack();
        }
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Gọi hàm TakeHit trong enemy
            enemy.GetComponent<EnemyFollow>().TakeHit(transform.position, knockbackForce);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
