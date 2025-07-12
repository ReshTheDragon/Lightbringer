using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class EndingSceneController : MonoBehaviour
{
    public Light2D globalLight;
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

    public GameObject winFinalDialog; // Dialog "You Win"
    public Button exitButton;         // Nút Exit
    public Button newGameButton;      // Nút New Game

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
        winFinalDialog.SetActive(false);

        // Gán sự kiện cho các nút trong WinFinalDialog
        exitButton.onClick.AddListener(ExitGame);
        newGameButton.onClick.AddListener(NewGame);


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
    void ShowFinalWinDialog()
    {
        winFinalDialog.SetActive(true); // Hiện final dialog
        StartCoroutine(FadeInLight());  // Làm sáng dần
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
        StartCoroutine(WaitForLightCoreDialog());
    }

    void CancelOpenChest()
    {
        winDialog.SetActive(false); // Đóng dialog hỏi mở rương
    }

    private IEnumerator WaitForLightCoreDialog()
    {
        portal.SetActive(true);
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Start");
        }
        yield return new WaitForSeconds(1f);
        portal.SetActive(false);
        //Hiện lightCoreDialog
        lightCoreDialog.SetActive(true);

        yield return new WaitForSeconds(5f);
        lightCoreDialog.SetActive(false);
        
        lightOrb.SetActive(false);
        ShowFinalWinDialog();
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
        ShowFinalWinDialog();
    }

    IEnumerator FadeInLight()
    {
        float targetIntensity = 2f; // Sáng hơn bình thường
        float speed = 0.5f;

        while (globalLight != null && globalLight.intensity < targetIntensity)
        {
            globalLight.intensity += speed * Time.deltaTime;
            yield return null;
        }
    }

    void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void NewGame()
    {
        Debug.Log("Starting new game");
        PlayerPrefs.DeleteAll(); // Xoá dữ liệu
        SceneManager.LoadScene("GameScene"); // Chơi lại từ đầu
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