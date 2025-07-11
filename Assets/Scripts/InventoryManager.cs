using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryUI;
    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;
    public InventorySlot hoveredSlot;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ClearAllSlots();
        LoadInventory(); // Load saved inventory

        // Check if slots are assigned in the inspector
        if (inventorySlots.Length == 0)
        {
            Debug.LogError("Inventory slots are not assigned in the InventoryManager!");
        }
        if (hotbarSlots.Length == 0)
        {
            Debug.LogError("Hotbar slots are not assigned in the InventoryManager!");
        }
    }

    public void ClearAllSlots()
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
            else
            {
                Debug.LogError("A hotbar slot is null!");
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
            else
            {
                Debug.LogError("An inventory slot is null!");
            }
        }
    }

    public static bool isInventoryOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            isInventoryOpen = inventoryUI.activeSelf;
        }

        // Handle Shift + Number key presses to move items to the hotbar
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwapHoveredItemWithHotbar(i);
                }
            }
        }
    }

    // Save inventory data to PlayerPrefs
    public void SaveInventory()
    {
        // Save hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].currentItem != null)
            {
                PlayerPrefs.SetString($"Hotbar_{i}_Item", hotbarSlots[i].currentItem.itemName);
                PlayerPrefs.SetInt($"Hotbar_{i}_Quantity", hotbarSlots[i].quantity);
            }
            else
            {
                PlayerPrefs.DeleteKey($"Hotbar_{i}_Item");
                PlayerPrefs.DeleteKey($"Hotbar_{i}_Quantity");
            }
        }

        // Save inventory
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].currentItem != null)
            {
                PlayerPrefs.SetString($"Inventory_{i}_Item", inventorySlots[i].currentItem.itemName);
                PlayerPrefs.SetInt($"Inventory_{i}_Quantity", inventorySlots[i].quantity);
            }
            else
            {
                PlayerPrefs.DeleteKey($"Inventory_{i}_Item");
                PlayerPrefs.DeleteKey($"Inventory_{i}_Quantity");
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Inventory saved!");
    }

    // Load inventory data from PlayerPrefs
    public void LoadInventory()
    {
        // Load hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            string itemName = PlayerPrefs.GetString($"Hotbar_{i}_Item", "");
            if (!string.IsNullOrEmpty(itemName))
            {
                InventoryItem item = FindItemByName(itemName);
                if (item != null)
                {
                    int quantity = PlayerPrefs.GetInt($"Hotbar_{i}_Quantity", 1);
                    hotbarSlots[i].AddItem(item, quantity);
                }
            }
        }

        // Load inventory
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            string itemName = PlayerPrefs.GetString($"Inventory_{i}_Item", "");
            if (!string.IsNullOrEmpty(itemName))
            {
                InventoryItem item = FindItemByName(itemName);
                if (item != null)
                {
                    int quantity = PlayerPrefs.GetInt($"Inventory_{i}_Quantity", 1);
                    inventorySlots[i].AddItem(item, quantity);
                }
            }
        }

        Debug.Log("Inventory loaded!");
    }

    // Find item by name (you need to implement this based on your item system)
    private InventoryItem FindItemByName(string itemName)
    {
        // This is a simple implementation - you might want to use a more sophisticated system
        // like a ScriptableObject database or item registry
        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
        foreach (InventoryItem item in allItems)
        {
            if (item.itemName == itemName)
            {
                return item;
            }
        }
        return null;
    }

    // Auto-save when changing scenes
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveInventory();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveInventory();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveInventory();
        }
    }

    void SwapHoveredItemWithHotbar(int hotbarIndex)
    {
        if (hoveredSlot != null && System.Array.IndexOf(inventorySlots, hoveredSlot) != -1)
        {
            // Get items from hovered inventory slot and target hotbar slot
            InventoryItem hoveredItem = hoveredSlot.currentItem;
            int hoveredQuantity = hoveredSlot.quantity;

            InventoryItem hotbarItem = hotbarSlots[hotbarIndex].currentItem;
            int hotbarQuantity = hotbarSlots[hotbarIndex].quantity;

            // Perform the swap
            if (hotbarItem != null)
            {
                hoveredSlot.AddItem(hotbarItem, hotbarQuantity);
            }
            else
            {
                hoveredSlot.ClearSlot();
            }

            if (hoveredItem != null)
            {
                hotbarSlots[hotbarIndex].AddItem(hoveredItem, hoveredQuantity);
            }
            else
            {
                hotbarSlots[hotbarIndex].ClearSlot();
            }

            SaveInventory(); // Save after swapping
        }
    }

    public bool AddItemToHotbar(InventoryItem item, int amount = 1)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot.currentItem == item)
            {
                slot.AddItem(item, amount);
                SaveInventory(); // Save after adding
                return true;
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot.currentItem == null)
            {
                Debug.Log("Đã add vào hotbar: " + item.itemName);
                slot.AddItem(item, amount);
                SaveInventory(); // Save after adding
                return true;
            }
        }
        Debug.Log("Hotbar full!");
        return false;
    }

    public void AddItemToInventory(InventoryItem item, int amount = 1)
    {
        Debug.Log($"Attempting to add {item.itemName} to inventory. Inventory slots length: {inventorySlots.Length}");

        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.currentItem == item)
            {
                Debug.Log($"Stacking {item.itemName} in existing slot.");
                slot.AddItem(item, amount);
                SaveInventory(); // Save after adding
                return;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.currentItem == null)
            {
                Debug.Log($"Adding {item.itemName} to empty inventory slot.");
                slot.AddItem(item, amount);
                SaveInventory(); // Save after adding
                return;
            }
        }
        Debug.Log("Inventory full! No space for " + item.itemName);
    }

    public void RemoveItemFromInventory(InventoryItem item, int amount = 1)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.currentItem == item)
            {
                slot.quantity -= amount;
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.quantityText.text = slot.quantity.ToString();
                }
                SaveInventory(); // Save after removing
                return;
            }
        }
    }

    public void RemoveItemFromHotbar(InventoryItem item)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot.currentItem == item)
            {
                slot.ClearSlot();
                SaveInventory(); // Save after removing
                return;
            }
        }
    }

    public void UseItemFromHotbar(int index)
    {
        if (index >= hotbarSlots.Length) return;

        var slot = hotbarSlots[index];
        if (slot.currentItem == null) return;

        var item = slot.currentItem;

        PlayerControl player = FindObjectOfType<PlayerControl>();

        switch (item.itemType)
        {
            case ItemType.ManaPotion:
                player.RestoreMana(20f);
                slot.quantity--;
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.quantityText.text = slot.quantity.ToString();
                }
                break;

            case ItemType.StaminaPotion:
                player.RestoreStamina(20f);
                slot.quantity--;
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.quantityText.text = slot.quantity.ToString();
                }
                break;

            case ItemType.HealthPotion:
                player.RestoreHealth(20f);
                slot.quantity--;
                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.quantityText.text = slot.quantity.ToString();
                }
                break;

            default:
                Debug.Log("Item không sử dụng được.");
                break;
        }

        SaveInventory(); // Save after using item
    }

    // Method to manually clear all saved data (for testing)
    public void ClearSavedData()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            PlayerPrefs.DeleteKey($"Hotbar_{i}_Item");
            PlayerPrefs.DeleteKey($"Hotbar_{i}_Quantity");
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            PlayerPrefs.DeleteKey($"Inventory_{i}_Item");
            PlayerPrefs.DeleteKey($"Inventory_{i}_Quantity");
        }

        PlayerPrefs.Save();
        Debug.Log("All saved inventory data cleared!");
    }
}