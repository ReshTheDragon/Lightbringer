using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BrightSceneController : MonoBehaviour
{
    public GameObject winFinalDialog; // Dialog "You Win"
    public Button exitButton; // Button "Exit"
    public Button newGameButton; // Button "New Game"

    void Start()
    {
        // Initialize: hide dialog and assign button events
        if (winFinalDialog != null)
        {
            winFinalDialog.SetActive(false); // Ensure dialog is hidden initially
            StartCoroutine(ShowWinDialogAfterDelay()); // Start delay for showing dialog
        }
        else
        {
            Debug.LogError("winFinalDialog is not assigned!");
        }

        // Assign button events
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(EndGame);
            Debug.Log("exitButton is assigned");
        }
        else
        {
            Debug.LogError("exitButton is not assigned!");
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(NewGame);
            Debug.Log("newGameButton is assigned");
        }
        else
        {
            Debug.LogError("newGameButton is not assigned!");
        }

        Debug.Log("BrightSceneController is running!");
    }

    private IEnumerator ShowWinDialogAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        if (winFinalDialog != null)
        {
            winFinalDialog.SetActive(true); // Show "You Win" dialog
            Debug.Log("winFinalDialog is shown after 2 seconds");
        }
    }

    void NewGame()
    {
        Debug.Log("Starting new game");
        PlayerPrefs.DeleteAll(); // Clear old data (optional)
        SceneManager.LoadScene("GameScene"); // Load main game scene
    }

    void EndGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}