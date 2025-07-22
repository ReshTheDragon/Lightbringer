using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName; // Tên scene cần chuyển sang

    [Header("Boss Condition")]
    public bool requireBossDefeat = true; // Có yêu cầu đánh boss không
    public DragonEnemy dragonBoss; // Kéo thả Dragon trực tiếp vào đây
    private bool isBossDefeated = false; // Trạng thái boss đã bị đánh bại

    [Header("Visual Feedback")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nếu không gán dragon trong Inspector, tự động tìm
        if (requireBossDefeat && dragonBoss == null)
        {
            dragonBoss = FindObjectOfType<DragonEnemy>();
        }

        UpdatePortalVisual();
    }

    private void Update()
    {
        // Kiểm tra trạng thái dragon
        if (requireBossDefeat && !isBossDefeated)
        {
            CheckBossStatus();
        }
    }

    private void CheckBossStatus()
    {
        // Kiểm tra bằng cách xem dragon có null không (đã bị destroy)
        if (dragonBoss == null)
        {
            isBossDefeated = true;
            UpdatePortalVisual();
            Debug.Log("✨ Dragon đã bị đánh bại! Portal mở khóa!");
        }
    }

    private void UpdatePortalVisual()
    {
        bool canUsePortal = !requireBossDefeat || isBossDefeated;

        // Thay đổi màu sắc portal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = canUsePortal ? unlockedColor : lockedColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Kiểm tra điều kiện trước khi chuyển scene
            if (!requireBossDefeat || isBossDefeated)
            {
                Debug.Log("🚪 Chuyển sang scene: " + targetSceneName);
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.Log("🔒 Bạn cần đánh bại Dragon trước khi có thể qua cổng này!");
                ShowLockedMessage();
            }
        }
    }

    private void ShowLockedMessage()
    {
        // Hiệu ứng rung lắc portal
        StartCoroutine(ShakePortal());
    }

    private System.Collections.IEnumerator ShakePortal()
    {
        if (spriteRenderer == null) yield break;

        Vector3 originalPosition = transform.position;
        float shakeDuration = 0.5f;
        float shakeIntensity = 0.1f;

        for (float t = 0; t < shakeDuration; t += Time.deltaTime)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * shakeIntensity;
            transform.position = originalPosition + shakeOffset;
            yield return null;
        }

        transform.position = originalPosition;
    }

    // Method để DragonEnemy gọi khi chết
    public void OnBossDefeated()
    {
        isBossDefeated = true;
        UpdatePortalVisual();
        Debug.Log("🎉 Dragon Boss đã bị đánh bại! Portal đã được mở khóa!");
    }
}