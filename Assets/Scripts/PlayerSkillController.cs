using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillController : MonoBehaviour
{
    public GameObject[] skillPrefabs;        // Prefab kỹ năng (4 cái)
    public Transform firePoint;              // Điểm bắn kỹ năng
    public float[] cooldowns = new float[4] { 5f, 10f, 15f, 20f };

    private float[] cooldownTimers = new float[4];

    public Image[] skillImages;              // Các ô UI để làm cooldown fill
    public float detectRadius = 10f;          // Phát hiện enemy gần

    void Update()
    {
        HandleCooldownUI();

        if (Input.GetKeyDown(KeyCode.U)) TryCastSkill(0);
        if (Input.GetKeyDown(KeyCode.I)) TryCastSkill(1);
        if (Input.GetKeyDown(KeyCode.O)) TryCastSkill(2);
        if (Input.GetKeyDown(KeyCode.P)) TryCastSkill(3);

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
                cooldownTimers[i] -= Time.deltaTime;
        }
    }

    void TryCastSkill(int index)
    {
        if (index >= skillPrefabs.Length || cooldownTimers[index] > 0f)
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
            if (skillImages[i] != null)
            {
                float fill = cooldownTimers[i] / cooldowns[i];
                skillImages[i].fillAmount = fill;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
