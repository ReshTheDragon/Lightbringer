using UnityEngine;

public class InventorySetup : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;
    public GameObject inventoryUI;

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager instance is null in InventorySetup!");
            return;
        }

        Debug.Log($"InventorySetup: Assigning {inventorySlots?.Length ?? 0} inventory slots and {hotbarSlots?.Length ?? 0} hotbar slots");
        InventoryManager.Instance.inventorySlots = inventorySlots ?? new InventorySlot[0];
        InventoryManager.Instance.hotbarSlots = hotbarSlots ?? new InventorySlot[0];

        if (inventoryUI == null)
        {
            inventoryUI = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
            Debug.Log($"InventoryUI {(inventoryUI != null ? "found: " + inventoryUI.name : "not found!")}");
        }

        InventoryManager.Instance.inventoryUI = inventoryUI;

        if (inventoryUI == null || inventorySlots.Length == 0 || hotbarSlots.Length == 0)
        {
            Debug.LogError("InventorySetup failed: Missing inventoryUI or slots!");
            return;
        }

        InventoryManager.Instance.ClearAllSlots();
        InventoryManager.Instance.LoadInventoryFromJSON();
        Debug.Log("Inventory and hotbar slots have been set up successfully.");
    }
}