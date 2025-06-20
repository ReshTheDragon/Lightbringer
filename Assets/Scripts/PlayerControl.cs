using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
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

        // Chuyển trạng thái Idle/Walk
        bool isWalking = move.magnitude > 0;
        animator.SetBool("IsWalking", isWalking);

        // Lật nhân vật theo hướng trái/phải
        if (moveX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("IsAttacking");
        }
    }
}
