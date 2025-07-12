using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryUI;
    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;
    public InventorySlot hoveredSlot;

    private void Awake()
    {
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

        if (inventorySlots.Length == 0)
            Debug.LogError("Inventory slots are not assigned in the InventoryManager!");
        if (hotbarSlots.Length == 0)
            Debug.LogError("Hotbar slots are not assigned in the InventoryManager!");
    }

    public void ClearAllSlots()
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot != null && slot.gameObject != null)
                slot.ClearSlot();
        }

        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.gameObject != null)
                slot.ClearSlot();
        }
    }

    public static bool isInventoryOpen = false;

    private void Update()
    {
        // Nếu inventoryUI bị null thì tìm lại trong scene
        if (inventoryUI == null)
        {
            inventoryUI = GameObject.Find("InventoryPanel");
            if (inventoryUI == null)
            {
                // Nếu chưa tìm thấy thì ngừng xử lý phím I luôn
                return;
            }
        }

        // Toggle Inventory bằng phím I
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            isInventoryOpen = inventoryUI.activeSelf;
            Debug.Log("Inventory toggled: " + isInventoryOpen);
        }

        // Shift + 1/2/3 để swap inventory với hotbar
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


    private InventoryItem FindItemByName(string itemName)
    {
        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
        foreach (InventoryItem item in allItems)
        {
            if (item.itemName == itemName)
                return item;
        }
        return null;
    }

    void SwapHoveredItemWithHotbar(int hotbarIndex)
    {
        if (hoveredSlot != null && System.Array.IndexOf(inventorySlots, hoveredSlot) != -1)
        {
            if (hotbarSlots[hotbarIndex] == null || hotbarSlots[hotbarIndex].gameObject == null) return;

            InventoryItem hoveredItem = hoveredSlot.currentItem;
            int hoveredQuantity = hoveredSlot.quantity;

            InventoryItem hotbarItem = hotbarSlots[hotbarIndex].currentItem;
            int hotbarQuantity = hotbarSlots[hotbarIndex].quantity;

            if (hotbarItem != null)
                hoveredSlot.AddItem(hotbarItem, hotbarQuantity);
            else
                hoveredSlot.ClearSlot();

            if (hoveredItem != null)
                hotbarSlots[hotbarIndex].AddItem(hoveredItem, hoveredQuantity);
            else
                hotbarSlots[hotbarIndex].ClearSlot();
        }
    }

    public bool AddItemToHotbar(InventoryItem item, int amount = 1)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.AddItem(item, amount);
                return true;
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                Debug.Log("Đã add vào hotbar: " + item.itemName);
                slot.AddItem(item, amount);
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
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                Debug.Log($"Stacking {item.itemName} in existing slot.");
                slot.AddItem(item, amount);
                return;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                Debug.Log($"Adding {item.itemName} to empty inventory slot.");
                slot.AddItem(item, amount);
                return;
            }
        }

        Debug.Log("Inventory full! No space for " + item.itemName);
    }

    public void RemoveItemFromInventory(InventoryItem item, int amount = 1)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.quantity -= amount;
                if (slot.quantity <= 0)
                    slot.ClearSlot();
                else
                    slot.quantityText.text = slot.quantity.ToString();

                return;
            }
        }
    }

    public void RemoveItemFromHotbar(InventoryItem item)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.ClearSlot();
                return;
            }
        }
    }

    public void UseItemFromHotbar(int index)
    {
        if (index >= hotbarSlots.Length) return;
        var slot = hotbarSlots[index];
        if (slot == null || slot.gameObject == null) return;
        if (slot.currentItem == null) return;

        var item = slot.currentItem;
        PlayerControl player = FindObjectOfType<PlayerControl>();

        switch (item.itemType)
        {
            case ItemType.ManaPotion:
                player.RestoreMana(20f);
                slot.quantity--;
                break;

            case ItemType.StaminaPotion:
                player.RestoreStamina(20f);
                slot.quantity--;
                break;

            case ItemType.HealthPotion:
                player.RestoreHealth(20f);
                slot.quantity--;
                break;

            default:
                Debug.Log("Item không sử dụng được.");
                break;
        }

        if (slot.quantity <= 0)
            slot.ClearSlot();
        else
            slot.quantityText.text = slot.quantity.ToString();
    }
}
