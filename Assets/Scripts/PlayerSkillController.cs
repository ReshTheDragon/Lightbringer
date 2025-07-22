using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillController : MonoBehaviour
{
    public GameObject[] skillPrefabs;        // Prefab kỹ năng (4 cái)
    public Transform firePoint;              // Điểm bắn kỹ năng
    public float[] cooldowns = new float[4] { 5f, 10f, 15f, 20f };
    private float[] cooldownTimers = new float[4];
    public Image[] skillImages;              // Các ô UI để làm cooldown fill
    public float detectRadius = 10f;         // Phát hiện enemy gần

    // Sử dụng static để lưu trạng thái mở khóa skill trong phiên chơi
    private static bool[] skillsUnlocked = new bool[4] { true, false, false, false }; // U mở, I, O, P ẩn
    private GameObject currentBook;          // Theo dõi cuốn sách đang chạm

    void Start()
    {
        // Khởi tạo cooldownTimers
        cooldownTimers = new float[4];
        // Cập nhật UI ban đầu
        UpdateSkillUI();
    }

    void Update()
    {
        HandleCooldownUI();

        // Chỉ cho phép nhấn phím nếu skill đã được mở khóa
        if (Input.GetKeyDown(KeyCode.U) && skillsUnlocked[0]) TryCastSkill(0);
        if (Input.GetKeyDown(KeyCode.I) && skillsUnlocked[1]) TryCastSkill(1);
        if (Input.GetKeyDown(KeyCode.O) && skillsUnlocked[2]) TryCastSkill(2);
        if (Input.GetKeyDown(KeyCode.P) && skillsUnlocked[3]) TryCastSkill(3);

        // Kiểm tra nhấn E để nhặt sách
        if (Input.GetKeyDown(KeyCode.E) && currentBook != null)
        {
            if (currentBook.CompareTag("Book1"))
            {
                skillsUnlocked[1] = true; // Mở khóa skill I
                Destroy(currentBook);
                currentBook = null;
                UpdateSkillUI(); // Cập nhật UI sau khi mở khóa
            }
            else if (currentBook.CompareTag("Book2"))
            {
                skillsUnlocked[2] = true; // Mở khóa skill O
                Destroy(currentBook);
                currentBook = null;
                UpdateSkillUI(); // Cập nhật UI sau khi mở khóa
            }
            else if (currentBook.CompareTag("Book3"))
            {
                skillsUnlocked[3] = true; // Mở khóa skill P
                Destroy(currentBook);
                currentBook = null;
                UpdateSkillUI(); // Cập nhật UI sau khi mở khóa
            }
        }

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
                cooldownTimers[i] -= Time.deltaTime;
        }
    }

    void TryCastSkill(int index)
    {
        if (index >= skillPrefabs.Length || cooldownTimers[index] > 0f || !skillsUnlocked[index])
            return;

        Vector2 direction = GetTargetDirection();
        GameObject skill = Instantiate(skillPrefabs[index], firePoint.position, Quaternion.identity);
        skill.GetComponent<SkillBehaviour>().Initialize(direction);
        cooldownTimers[index] = cooldowns[index];
    }

    Vector2 GetTargetDirection()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        Transform nearestEnemy = null;
        float shortestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("DragonBoss"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearestEnemy = hit.transform;
                }
            }
        }

        if (nearestEnemy != null)
            return (nearestEnemy.position - transform.position).normalized;

        return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    }

    void HandleCooldownUI()
    {
        for (int i = 0; i < skillImages.Length; i++)
        {
            if (skillImages[i] != null && skillsUnlocked[i])
            {
                float fill = cooldownTimers[i] / cooldowns[i];
                skillImages[i].fillAmount = fill;
            }
        }
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < skillImages.Length; i++)
        {
            if (skillImages[i] != null)
            {
                skillImages[i].gameObject.SetActive(skillsUnlocked[i]); // Ẩn hoàn toàn UI của skill chưa mở
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Book1") || collision.CompareTag("Book2") || collision.CompareTag("Book3"))
        {
            currentBook = collision.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == currentBook)
        {
            currentBook = null;
        }
    }
}