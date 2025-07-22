using UnityEngine;

public class InventorySetup : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;
    public GameObject inventoryUI;

    void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.inventorySlots = inventorySlots;
            InventoryManager.Instance.hotbarSlots = hotbarSlots;

            // Nếu inventoryUI chưa gán thì tìm trong scene
            if (inventoryUI == null)
            {
                inventoryUI = GameObject.Find("InventoryPanel");
            }

            InventoryManager.Instance.inventoryUI = inventoryUI;

            InventoryManager.Instance.ClearAllSlots();
            InventoryManager.Instance.LoadInventoryFromJSON(); // Đảm bảo load lại sau khi setup slot
            Debug.Log("Inventory and hotbar slots have been set up successfully.");
        }
    }
}
