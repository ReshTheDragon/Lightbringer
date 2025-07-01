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

    public float maxStamina = 100f;
    private float currentStamina;

    private PlayerHUDController hudController;

    private bool isPauseMenuLoaded = false;

    public GameObject projectilePrefab;  // Prefab viên chưởng
    public Transform chargePoint;        // Vị trí bắn chưởng

    public float maxMana = 100f;
    private float currentMana;

    private bool isCharging = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Stamina khởi tạo
        currentStamina = maxStamina;

        // Tìm HUD Controller
        hudController = FindObjectOfType<PlayerHUDController>();

        // Cập nhật UI ban đầu
        hudController.UpdateStamina(currentStamina / maxStamina);

        currentMana = maxMana;
        hudController.UpdateMana(currentMana / maxMana);

    }

    void Update()
    {
        if (InventoryManager.isInventoryOpen)
        {
            return; // Don't process any input if inventory is open
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseMenuLoaded)
            {
                SceneManager.LoadScene("GamePauseMenu", LoadSceneMode.Additive);
                Time.timeScale = 0f;
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

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
            attackPoint.localPosition = localPos;
        }

        // Kiểm tra stamina đủ mới cho đánh
        if (Input.GetMouseButtonDown(0))
        {
            if (currentStamina >= 17f)
            {
                animator.SetTrigger("IsAttacking");
                Attack();

                // Trừ stamina
                currentStamina -= 17f;
                if (currentStamina < 0) currentStamina = 0;

                // Update UI
                hudController.UpdateStamina(currentStamina / maxStamina);
            }
            else
            {
                Debug.Log("Not enough stamina!");
            }
        }

        // Hồi stamina từ từ mỗi frame
        if (currentStamina < maxStamina)
        {
            currentStamina += 1f * Time.deltaTime;  // tốc độ hồi 5/s
            if (currentStamina > maxStamina) currentStamina = maxStamina;

            hudController.UpdateStamina(currentStamina / maxStamina);
        }


        // Bắt đầu giữ chuột phải để charge
        if (Input.GetMouseButtonDown(1))
        {
            if (currentMana > 0)
            {
                isCharging = true;
                animator.SetBool("IsCharging", true); // nếu có animation charge thì kích hoạt
            }
            else
            {
                Debug.Log("Not enough mana to start charging!");
            }
        }

        

        // Nhả chuột phải để bắn
        if (Input.GetMouseButtonUp(1) && isCharging)
        {
            ShootSkill();
            isCharging = false;
            animator.SetBool("IsCharging", false);
        }

        if (currentMana < maxMana)
        {
            currentMana += 1f * Time.deltaTime;
            if (currentMana > maxMana) currentMana = maxMana;

            hudController.UpdateMana(currentMana / maxMana);
        }

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
            attackPoint.localPosition = localPos;
        }

        if (chargePoint != null)
        {
            Vector3 chargeLocalPos = chargePoint.localPosition;
            chargeLocalPos.x = Mathf.Abs(chargeLocalPos.x) * (spriteRenderer.flipX ? -1 : 1);
            chargePoint.localPosition = chargeLocalPos;
        }

        // Use hotbar items
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseHotbarItem(i);
            }
        }
    }

    void UseHotbarItem(int index)
    {
        InventoryManager.Instance.UseItemFromHotbar(index);
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyFollow>().TakeHit(transform.position, knockbackForce, 25f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void ShootSkill()
    {
        if (currentMana >= 17f)
        {
            // Tạo chưởng
            GameObject proj = Instantiate(projectilePrefab, chargePoint.position, Quaternion.identity);

            // Tính hướng bắn về phía chuột
            Vector2 shootDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - chargePoint.position).normalized;

            // Gán vận tốc
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDir * 25f;

            // Trừ mana
            currentMana -= 17f;
            if (currentMana < 0) currentMana = 0;
            hudController.UpdateMana(currentMana / maxMana);

            Debug.Log("Skill fired!");
        }
        else
        {
            Debug.Log("Not enough mana to shoot!");
        }
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;
        hudController.UpdateMana(currentMana / maxMana);
        Debug.Log("Đã hồi " + amount + " mana");
    }

    public void RestoreStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
        hudController.UpdateStamina(currentStamina / maxStamina);
        Debug.Log("Đã hồi " + amount + " stamina");
    }

}
