using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName; // Tên scene cần chuyển sang

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Load scene mới
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
