using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSetup : MonoBehaviour
{
    void Start()
    {
        // Set trạng thái thắng hoặc thua để thử nghiệm
        PlayerPrefs.SetInt("IsWin", 1); // Thắng
        // PlayerPrefs.SetInt("IsWin", 0); // Thua (bỏ comment để thử trường hợp thua)
        Debug.Log("IsWin set to: " + PlayerPrefs.GetInt("IsWin"));
        SceneManager.LoadScene("EndingScene"); // Tải scene EndingScene
    }
}