using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public InventoryItem item;
    public int quantity = 1;
    public GameObject pressEText;

    private bool isPlayerNear = false;

    void Start()
    {
        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }
        if (item == null)
        {
            Debug.LogError($"ItemPickup on {gameObject.name} has no item assigned!");
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (item == null)
            {
                Debug.LogError($"Cannot pick up item on {gameObject.name}: No item assigned!");
                return;
            }

            Debug.Log($"Attempting to pick up item: {item.itemName}, Quantity: {quantity}");
            Debug.Log($"Inventory slots: {InventoryManager.Instance.inventorySlots.Length}, Hotbar slots: {InventoryManager.Instance.hotbarSlots.Length}");

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