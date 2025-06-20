using UnityEngine;
using UnityEngine.SceneManagement;
public class GameUi : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    public void StartGame()
    {
        gameManager.StartGame();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ResumeGame()
    {
        gameManager.ResumeGame();
    }
    public void ShowMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
