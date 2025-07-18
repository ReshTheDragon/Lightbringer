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


    public float maxMana = 100f;
    private float currentMana;

    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject[] skillPrefabs = new GameObject[4]; // gán trong Inspector
    public Transform skillSpawnPoint;                      // giống như FirePoint
    private float[] skillManaCosts = new float[4] { 10f, 20f, 30f, 40f };
    private float[] skillCooldowns = new float[4] { 5f, 10f, 15f, 20f };
    private float[] skillCooldownTimers = new float[4];
    private Vector2 lastMoveDirection = Vector2.right; // mặc định hướng phải


    [SerializeField] private AudioManager audioManager;
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Khởi tạo máu
        currentHealth = maxHealth;

        // Stamina khởi tạo
        currentStamina = maxStamina;

        // Tìm HUD Controller
        hudController = FindObjectOfType<PlayerHUDController>();

        // Cập nhật UI ban đầu
        hudController.UpdateStamina(currentStamina / maxStamina);

        currentMana = maxMana;
        hudController.UpdateMana(currentMana / maxMana);

        hudController.UpdateHealth(currentHealth / maxHealth);

        audioManager.playBackGroundClip();
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
        // Flip skillSpawnPoint theo hướng nhân vật
        if (skillSpawnPoint != null)
        {
            Vector3 localPos = skillSpawnPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * (spriteRenderer.flipX ? -1 : 1);
            skillSpawnPoint.localPosition = localPos;
        }

        if (move.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = move;
        }

        // Kiểm tra stamina đủ mới cho đánh
        if (Input.GetKeyDown(KeyCode.J))
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

        // Use hotbar items
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseHotbarItem(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.U)) TryCastSkill(0);
        if (Input.GetKeyDown(KeyCode.I)) TryCastSkill(1);
        if (Input.GetKeyDown(KeyCode.O)) TryCastSkill(2);
        if (Input.GetKeyDown(KeyCode.P)) TryCastSkill(3);

        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
                skillCooldownTimers[i] -= Time.deltaTime;
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
            // Cả EnemyFollow và BossController đều có TakeHit()
            if (enemy.GetComponent<EnemyFollow>() != null)
            {
                enemy.GetComponent<EnemyFollow>().TakeHit(transform.position, knockbackForce, 25f);
            }
            else if (enemy.GetComponent<BossController>() != null)
            {
                enemy.GetComponent<BossController>().TakeDamage(25);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
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

    public void RestoreHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        hudController.UpdateHealth(currentHealth / maxHealth);
        Debug.Log("Đã hồi " + amount + " máu");
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Cập nhật UI
        hudController.UpdateHealth(currentHealth / maxHealth);
        //Debug.Log("Player took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        animator.SetTrigger("IsDead");

        // Load lại scene GroundLevel sau một chút delay để animation kịp chạy
        Invoke(nameof(LoadGroundLevelScene), 0.5f);
    }

    void LoadGroundLevelScene()
    {
        // Bỏ pause nếu có
        Time.timeScale = 1f;

        // Load scene GroundLevel
        //SceneManager.LoadScene("Ground Level");
        PlayerPrefs.SetString("LastLevel", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("IsWin", 0);
        SceneManager.LoadScene("EndingScene");
    }

    void TryCastSkill(int index)
    {
        if (index < 0 || index >= skillPrefabs.Length) return;

        if (skillCooldownTimers[index] > 0f)
        {
            Debug.Log("Skill " + index + " is on cooldown.");
            return;
        }

        float manaCost = skillManaCosts[index];
        if (currentMana < manaCost)
        {
            Debug.Log("Not enough mana to cast skill " + index);
            return;
        }

        currentMana -= manaCost;
        hudController.UpdateMana(currentMana / maxMana);

        // Hướng bắn: kẻ địch gần nhất, nếu không có thì dùng hướng đi gần nhất
        Vector2 dir = FindEnemyDirection();
        if (dir == Vector2.zero)
        {
            dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }


        GameObject skill = Instantiate(skillPrefabs[index], skillSpawnPoint.position, Quaternion.identity);
        SkillBehaviour sb = skill.GetComponent<SkillBehaviour>();
        if (sb != null)
        {
            float damage = skillManaCosts[index];
            sb.Initialize(dir, damage);
        }

        skillCooldownTimers[index] = skillCooldowns[index];
    }


    Vector2 FindEnemyDirection()
    {
        float radius = 5f;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius);

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col.transform;
                }
            }
        }

        if (nearest != null)
            return (nearest.position - transform.position).normalized;
        return Vector2.zero;
    }

}
