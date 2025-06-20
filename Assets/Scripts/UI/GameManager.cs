using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameUi;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject pauseMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f; // Ensure the game is not paused when showing the main menu
    }
    public void ShowGameOverMenu()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f; // Pause the game when showing the game over menu
    }
    public void ShowPauseMenu()
    {
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Pause the game when showing the pause menu
    }
    public void StartGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        SceneManager.LoadScene(1);
        Time.timeScale = 1f; // Resume the game when starting
    }
    public void ResumeGame()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        Time.timeScale = 1f; // Resume the game when resuming
    }
}
