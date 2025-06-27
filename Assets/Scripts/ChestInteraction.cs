using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    public Sprite openedChestSprite;
    public GameObject potionPrefab;
    public Vector3 spawnOffset = Vector3.zero;
    public GameObject pressEText; // Thêm biến tham chiếu chữ E

    private bool isPlayerNear = false;
    private SpriteRenderer spriteRenderer;
    private bool isOpened = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (pressEText != null)
        {
            pressEText.SetActive(false); // Tắt khi start
        }
    }

    void Update()
    {
        if (isPlayerNear && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        spriteRenderer.sprite = openedChestSprite;
        isOpened = true;
        Debug.Log("Chest opened!");

        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }

        if (potionPrefab != null)
        {
            Instantiate(potionPrefab, transform.position + spawnOffset, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isOpened && pressEText != null)
            {
                pressEText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (pressEText != null)
            {
                pressEText.SetActive(false);
            }
        }
    }
}
