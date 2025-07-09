using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public InventoryItem item;
    public int quantity = 1;

    public GameObject pressEText; // Thêm biến tham chiếu chữ E

    private bool isPlayerNear = false;

    void Start()
    {
        if (pressEText != null)
        {
            pressEText.SetActive(false); // Tắt khi start
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (InventoryManager.Instance.AddItemToHotbar(item, quantity))
            {
                Debug.Log($"Picked up {quantity} {item.itemName}(s) to hotbar!");
                Destroy(gameObject);
            }
            else
            {
                InventoryManager.Instance.AddItemToInventory(item, quantity);
                Debug.Log($"Picked up {quantity} {item.itemName}(s) to inventory!");
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (pressEText != null)
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
