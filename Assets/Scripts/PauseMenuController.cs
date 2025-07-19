using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public Button continueButton;
    public Button exitButton;
    //public Animation menuAnimation;

    [Header("Settings")]
  
    public string menuSceneName = "Main Menu";
    public int menuSceneIndex = 0;

    private bool isPaused = false;
    private bool isMenuOpen = false;

    void Start()
    {
        // Đảm bảo pause menu bị ẩn khi bắt đầu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Thêm listeners cho buttons
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitToMenu);
        }

        // Đảm bảo game không bị pause khi bắt đầu
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Kiểm tra phím ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (isMenuOpen) return;

        isPaused = true;
        isMenuOpen = true;
        Time.timeScale = 0f; // Dừng thời gian game

        // Hiển thị pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);

    
        }

        // Unlock cursor để có thể click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    public void ContinueGame()
    {
        if (!isMenuOpen) return;

        StartCoroutine(ContinueGameCoroutine());
    }

    private IEnumerator ContinueGameCoroutine()
    {
        // Chạy animation fade out nếu có
     

        // Ẩn pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        isPaused = false;
        isMenuOpen = false;
        Time.timeScale = 1f; // Khôi phục thời gian game

        // Lock cursor lại nếu cần (tùy thuộc vào game của bạn)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        Debug.Log("Game Continued");
        return null;
    }

    public void ExitToMenu()
    {
        if (!isMenuOpen) return;

        // Lưu game trước khi thoát
        //GameSaveSystem.SaveGame();

        // Khôi phục thời gian trước khi chuyển scene
        Time.timeScale = 1f;

        // Chuyển về menu chính
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(menuSceneIndex);
        }

        Debug.Log("Exiting to menu...");
    }

    void OnDestroy()
    {
        // Đảm bảo thời gian được khôi phục khi object bị destroy
        Time.timeScale = 1f;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Tự động pause khi app bị pause (mobile)
        if (pauseStatus && !isPaused)
        {
            PauseGame();
        }
    }
}
