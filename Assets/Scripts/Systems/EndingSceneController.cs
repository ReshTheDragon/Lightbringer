using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingSceneController : MonoBehaviour
{
    public GameObject player; // Nhân vật
    public Transform podiumPosition; // Vị trí bục
    public GameObject chest; // Rương
    public GameObject lightOrb; // Quả cầu ánh sáng
    public GameObject loseDialog; // Dialog "You Lose"
    public GameObject winDialog; // Dialog hỏi mở rương
    public GameObject lightCoreDialog; // Dialog "Chúc mừng bạn đã nhận lõi ánh sáng"
    public GameObject portal; // GameObject vòng xoay xuyên không
    public Button retryButton; // Button "Chơi lại"
    public Button endGameButton; // Button "End Game"
    public Button openChestButton; // Button "Mở rương"
    public Button cancelButton; // Button "Hủy"
    public Animator lightOrbAnimator; // Animator của quả cầu ánh sáng
    public Animator portalAnimator; // Animator cho vòng xoay xuyên không

    private bool isWin = true; // Biến kiểm tra thắng/thua
    private bool isMovingToPodium = true; // Trạng thái di chuyển đến bục
    private float moveSpeed = 5f; // Tốc độ di chuyển của nhân vật

    void Start()
    {
        // Ẩn tất cả dialog và portal ban đầu
        loseDialog.SetActive(false);
        winDialog.SetActive(false);
        lightCoreDialog.SetActive(false);
        lightOrb.SetActive(false);
        portal.SetActive(false); // Ẩn portal ban đầu

        // Gán sự kiện cho các button
        retryButton.onClick.AddListener(RetryGame);
        endGameButton.onClick.AddListener(EndGame);
        openChestButton.onClick.AddListener(OpenChest);
        cancelButton.onClick.AddListener(CancelOpenChest);

        // Lấy trạng thái thắng/thua
        isWin = PlayerPrefs.GetInt("IsWin", 1) == 1;
        Debug.Log("EndingSceneController is running! IsWin: " + isWin);
    }

    void Update()
    {
        if (isMovingToPodium)
        {
            // Di chuyển nhân vật đến bục
            player.transform.position = Vector2.MoveTowards(
                player.transform.position,
                podiumPosition.position,
                moveSpeed * Time.deltaTime
            );

            // Khi nhân vật đến gần bục
            if (Vector2.Distance(player.transform.position, podiumPosition.position) < 0.1f)
            {
                isMovingToPodium = false;
                if (isWin)
                {
                    winDialog.SetActive(true); // Hiện dialog hỏi mở rương
                }
                else
                {
                    loseDialog.SetActive(true); // Hiện dialog "You Lose"
                }
            }
        }
    }

    void OpenChest()
    {
        winDialog.SetActive(false);
        StartCoroutine(WaitForChestAnimation()); // Bắt đầu chờ animation rương
    }

    private IEnumerator WaitForChestAnimation()
    {
        // Kích hoạt animation mở rương
        ChestOpener chestOpener = chest.GetComponent<ChestOpener>();
        if (chestOpener != null && chestOpener.animator != null)
        {
            chestOpener.OpenChest(); // Chạy animation "Open"
            Debug.Log("Chest animation started");

            // Chờ animation "Open" hoàn tất
            while (chestOpener.animator.GetCurrentAnimatorStateInfo(0).IsName("ChestOpen") &&
                   chestOpener.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null; // Chờ frame tiếp theo
            }
            Debug.Log("Chest animation finished");
        }
        else
        {
            Debug.LogWarning("ChestOpener or animator is not assigned, proceeding immediately");
        }

        // Sau khi rương mở, hiển thị lightOrb
        lightOrb.SetActive(true); // Hiện quả cầu ánh sáng
        if (lightOrbAnimator != null)
        {
            lightOrbAnimator.SetTrigger("Appear"); // Chạy animation "Appear"
            StartCoroutine(WaitForLightOrbAnimation()); // Chờ animation lightOrb
        }
        else
        {
            Debug.LogWarning("lightOrbAnimator is not assigned, showing lightCoreDialog immediately");
            StartCoroutine(WaitForLightCoreDialogDelay()); // Chuyển tiếp với độ trễ
        }
    }

    private IEnumerator WaitForLightOrbAnimation()
    {
        // Chờ animation Appear hoàn tất
        while (lightOrbAnimator != null && lightOrbAnimator.GetCurrentAnimatorStateInfo(0).IsName("LightOrbAppear") &&
               lightOrbAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null; // Chờ frame tiếp theo
        }
        Debug.Log("LightOrb animation finished");
        StartCoroutine(WaitForLightCoreDialogDelay()); // Chờ thêm 2 giây trước khi hiện dialog
    }

    private IEnumerator WaitForLightCoreDialogDelay()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây sau khi lightOrb xuất hiện
        ShowLightCoreDialog();
    }

    void CancelOpenChest()
    {
        winDialog.SetActive(false); // Đóng dialog hỏi mở rương
    }

    void ShowLightCoreDialog()
    {
        lightCoreDialog.SetActive(true);
        StartCoroutine(WaitForLightCoreDialog()); // Chờ trước khi chạy portal animation
    }

    private IEnumerator WaitForLightCoreDialog()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây để người chơi đọc dialog
        StartPortalAnimation();
    }

    void StartPortalAnimation()
    {
        portal.SetActive(true); // Bật portal trước khi chạy animation
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Start"); // Chạy animation vòng xoay
            StartCoroutine(WaitForPortalAnimation()); // Chờ animation hoàn tất
        }
        else
        {
            Debug.LogWarning("portalAnimator is not assigned, loading BrightScene immediately");
            LoadBrightScene();
        }
    }

    private IEnumerator WaitForPortalAnimation()
    {
        // Chờ animation Start hoàn tất
        while (portalAnimator != null && portalAnimator.GetCurrentAnimatorStateInfo(0).IsName("PortalStart") &&
               portalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null; // Chờ frame tiếp theo
        }
        Debug.Log("Portal animation finished");
        LoadBrightScene();
    }

    void LoadBrightScene()
    {
        SceneManager.LoadScene("BrightScene"); // Chuyển sang scene sáng
    }

    void RetryGame()
    {
        SceneManager.LoadScene("GameScene"); // Tên scene game chính
    }

    void EndGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}