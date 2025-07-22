using UnityEngine;

public class DragonEnemy : EnemyFollow
{
    private bool isWalking = false;

    [Header("Portal Integration")]
    public Portal linkedPortal; // Kéo thả portal vào đây trong Inspector
    [SerializeField] private AudioManager audioManager;

    protected override void Start()
    {
        base.Start();
        speed = 1.5f;
        attackDamage = 15f;
        chaseRadius = 7f;
        maxHealth = 150f;

        // Nếu không gán portal trong Inspector, tự động tìm
        if (linkedPortal == null)
        {
            linkedPortal = FindObjectOfType<Portal>();
        }
    }

    protected override void Update()
    {
        if (player == null || isDying) return;

        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool shouldWalk = distanceToPlayer < chaseRadius && !isPlayerInAttackRange;

        // Cập nhật animation IsWalk
        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            animator.SetBool("IsWalk", isWalking);
        }

        // Gọi Update của class cha
        base.Update();
    }

    public override void TakeHit(Vector3 attackerPos, float force, float damage)
    {
        // Dragon chỉ nhận 80% damage từ skill
        float reducedDamage = damage * 0.8f;
        base.TakeHit(attackerPos, force * 0.5f, reducedDamage);
        Debug.Log($"🐉 Dragon bị trúng chiêu và nhận {reducedDamage} damage!");

        // Tùy chọn: Thêm hiệu ứng hoặc hành vi riêng
        StartCoroutine(StunBriefly());
    }

    // Override method Die từ class cha (EnemyFollow)
    public override void Die()
    {
        Debug.LogWarning("🐉 Dragon Boss đã bị đánh bại!");


        // Thông báo cho portal rằng dragon boss đã chết
        if (linkedPortal != null)
        {
            linkedPortal.OnBossDefeated();
        }

        // Thêm hiệu ứng đặc biệt khi dragon chết
        StartCoroutine(DragonDeathEffect());

        // Gọi Die của class cha để xử lý logic chết cơ bản
        base.Die();
    }

    private System.Collections.IEnumerator StunBriefly()
    {
        speed = 0f; // dừng di chuyển
        yield return new WaitForSeconds(1f);
        speed = 1.5f; // trở lại bình thường
    }

    private System.Collections.IEnumerator DragonDeathEffect()
    {
        // Hiệu ứng đặc biệt khi dragon chết
        // Ví dụ: rung camera, particle effect, sound effect

        // Rung camera ngay lập tức
        if (Camera.main != null)
        {
            StartCoroutine(ShakeCamera());
        }

        // Có thể thêm particle effect, sound effect ở đây
        // Ví dụ:
        // AudioSource.PlayClipAtPoint(dragonDeathSound, transform.position);
        // Instantiate(deathParticleEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        Debug.Log("✨ Portal đã được mở khóa!");
    }

    private System.Collections.IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = Camera.main.transform.position;
        float shakeDuration = 2f;
        float shakeIntensity = 0.3f;

        for (float t = 0; t < shakeDuration; t += Time.deltaTime)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * shakeIntensity * (1 - t / shakeDuration);
            Camera.main.transform.position = originalPosition + shakeOffset;
            yield return null;
        }

        Camera.main.transform.position = originalPosition;
    }
}