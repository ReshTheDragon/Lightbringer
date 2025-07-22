using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class InventoryData
{
    public List<SlotData> hotbarSlots = new List<SlotData>();
    public List<SlotData> inventorySlots = new List<SlotData>();
}

[System.Serializable]
public class SlotData
{
    public string itemName;
    public int quantity;

    public SlotData(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject inventoryUI;
    public InventorySlot[] inventorySlots;
    public InventorySlot[] hotbarSlots;
    public InventorySlot hoveredSlot;

    private string savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "inventory.json");
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

        // Delay để đảm bảo UI đã được khởi tạo
        StartCoroutine(DelayedStart());

        if (inventorySlots.Length == 0)
            Debug.LogError("Inventory slots are not assigned in the InventoryManager!");
        if (hotbarSlots.Length == 0)
            Debug.LogError("Hotbar slots are not assigned in the InventoryManager!");
    }

    private System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);
        LoadInventoryFromJSON();
    }

    // Lưu inventory vào JSON file
    public void SaveInventoryToJSON()
    {
        InventoryData data = new InventoryData();

        // Lưu hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i] != null && hotbarSlots[i].currentItem != null)
            {
                data.hotbarSlots.Add(new SlotData(hotbarSlots[i].currentItem.itemName, hotbarSlots[i].quantity));
            }
            else
            {
                data.hotbarSlots.Add(new SlotData("", 0));
            }
        }

        // Lưu inventory
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i] != null && inventorySlots[i].currentItem != null)
            {
                data.inventorySlots.Add(new SlotData(inventorySlots[i].currentItem.itemName, inventorySlots[i].quantity));
            }
            else
            {
                data.inventorySlots.Add(new SlotData("", 0));
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Inventory saved to JSON: " + savePath);
    }

    // Tải inventory từ JSON file
    public void LoadInventoryFromJSON()
    {
        if (File.Exists(savePath))
        {
            Debug.Log("Loading inventory from: " + savePath);
            string json = File.ReadAllText(savePath);
            Debug.Log("JSON content: " + json);

            InventoryData data = JsonUtility.FromJson<InventoryData>(json);

            // Đợi một frame để đảm bảo slots đã được khởi tạo
            StartCoroutine(LoadInventoryCoroutine(data));
        }
        else
        {
            Debug.Log("No save file found. Starting with empty inventory.");
        }
    }

    private System.Collections.IEnumerator LoadInventoryCoroutine(InventoryData data)
    {
        yield return new WaitForEndOfFrame();

        // Refresh slot references before loading
        RefreshSlotReferences();

        // Tải hotbar với null checking
        for (int i = 0; i < data.hotbarSlots.Count && i < hotbarSlots.Length; i++)
        {
            // Check if slot exists and is not destroyed
            if (hotbarSlots[i] == null)
            {
                Debug.LogWarning($"Hotbar slot {i} is null, skipping");
                continue;
            }

            if (!string.IsNullOrEmpty(data.hotbarSlots[i].itemName))
            {
                InventoryItem item = FindItemByName(data.hotbarSlots[i].itemName);
                if (item != null)
                {
                    Debug.Log($"Loading hotbar slot {i}: {item.itemName} x{data.hotbarSlots[i].quantity}");
                    try
                    {
                        hotbarSlots[i].AddItem(item, data.hotbarSlots[i].quantity);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error adding item to hotbar slot {i}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Item not found: {data.hotbarSlots[i].itemName}");
                }
            }
        }

        // Tải inventory với null checking
        for (int i = 0; i < data.inventorySlots.Count && i < inventorySlots.Length; i++)
        {
            // Check if slot exists and is not destroyed
            if (inventorySlots[i] == null)
            {
                Debug.LogWarning($"Inventory slot {i} is null, skipping");
                continue;
            }

            if (!string.IsNullOrEmpty(data.inventorySlots[i].itemName))
            {
                InventoryItem item = FindItemByName(data.inventorySlots[i].itemName);
                if (item != null)
                {
                    Debug.Log($"Loading inventory slot {i}: {item.itemName} x{data.inventorySlots[i].quantity}");
                    try
                    {
                        inventorySlots[i].AddItem(item, data.inventorySlots[i].quantity);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error adding item to inventory slot {i}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Item not found: {data.inventorySlots[i].itemName}");
                }
            }
        }

        Debug.Log("Inventory loaded from JSON!");
    }

    // Xóa save file
    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted!");
        }
    }

    // Auto save khi chuyển scene hoặc thoát game
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveInventoryToJSON();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveInventoryToJSON();
    }

    private void OnDestroy()
    {
        SaveInventoryToJSON();
    }

    // Lưu tự động khi có thay đổi
    private void AutoSave()
    {
        SaveInventoryToJSON();
    }

    // Gọi khi scene sắp chuyển
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Khi scene mới load, tự động load inventory
        StartCoroutine(DelayedLoadAfterSceneChange());
    }

    private System.Collections.IEnumerator DelayedLoadAfterSceneChange()
    {
        yield return new WaitForSeconds(0.5f); // Đợi scene khởi tạo hoàn toàn

        // Tìm lại UI references
        if (inventoryUI == null)
            inventoryUI = GameObject.Find("InventoryPanel");

        // Tìm lại slot references nếu cần
        RefreshSlotReferences();

        // Load inventory
        LoadInventoryFromJSON();
    }

    // Tìm lại references của slots sau khi chuyển scene
    private void RefreshSlotReferences()
    {
        // Nếu slots bị mất reference, tìm lại
        if (inventorySlots == null || inventorySlots.Length == 0 || System.Array.Exists(inventorySlots, slot => slot == null))
        {
            GameObject inventoryPanel = GameObject.Find("InventoryPanel");
            if (inventoryPanel != null)
            {
                inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();
                Debug.Log($"Refreshed inventory slots: {inventorySlots.Length} found");
            }
        }

        if (hotbarSlots == null || hotbarSlots.Length == 0 || System.Array.Exists(hotbarSlots, slot => slot == null))
        {
            GameObject hotbarPanel = GameObject.Find("HotbarPanel");
            if (hotbarPanel != null)
            {
                hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>();
                Debug.Log($"Refreshed hotbar slots: {hotbarSlots.Length} found");
            }
        }
    }

    // Các phương thức cũ giữ nguyên...
    public void ClearAllSlots()
    {
        if (hotbarSlots != null)
        {
            foreach (var slot in hotbarSlots)
            {
                if (slot != null && slot != null && slot.gameObject != null)
                    slot.ClearSlot();
            }
        }

        if (inventorySlots != null)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot != null && slot != null && slot.gameObject != null)
                    slot.ClearSlot();
            }
        }
    }

    public static bool isInventoryOpen = false;

    private void Update()
    {
        if (inventoryUI == null)
        {
            inventoryUI = GameObject.Find("InventoryPanel");
            if (inventoryUI == null)
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            isInventoryOpen = inventoryUI.activeSelf;
            Debug.Log("Inventory toggled: " + isInventoryOpen);
        }

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

        // Phím F5 để lưu thủ công (giữ lại cho debug)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveInventoryToJSON();
        }

        // Phím F9 để load thủ công (giữ lại cho debug)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadInventoryFromJSON();
        }

        // Phím F6 để test tìm item
        if (Input.GetKeyDown(KeyCode.F6))
        {
            InventoryItem testItem = FindItemByName("Health");
            if (testItem != null)
                Debug.Log("Found Health item: " + testItem.name);
            else
                Debug.Log("Health item not found!");
        }
    }

    private InventoryItem FindItemByName(string itemName)
    {
        // Tìm trong Resources/Items
        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
        Debug.Log($"Found {allItems.Length} items in Resources/Items");

        foreach (InventoryItem item in allItems)
        {
            Debug.Log($"Checking item: {item.itemName} vs {itemName}");
            if (item.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Found matching item: {item.itemName}");
                return item;
            }
        }

        // Nếu không tìm thấy trong Resources, thử tìm trong toàn bộ project
        InventoryItem[] allProjectItems = FindObjectsOfType<InventoryItem>();
        foreach (InventoryItem item in allProjectItems)
        {
            if (item.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        Debug.LogWarning($"Item not found: {itemName}");
        return null;
    }

    // Các phương thức khác giữ nguyên như code gốc...
    void SwapHoveredItemWithHotbar(int hotbarIndex)
    {
        if (hoveredSlot != null && System.Array.IndexOf(inventorySlots, hoveredSlot) != -1)
        {
            if (hotbarSlots[hotbarIndex] == null || hotbarSlots[hotbarIndex] == null || hotbarSlots[hotbarIndex].gameObject == null) return;

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

            AutoSave(); // Lưu tự động
        }
    }

    public bool AddItemToHotbar(InventoryItem item, int amount = 1)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.AddItem(item, amount);
                AutoSave(); // Lưu tự động
                return true;
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                Debug.Log("Đã add vào hotbar: " + item.itemName);
                slot.AddItem(item, amount);
                AutoSave(); // Lưu tự động
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
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                Debug.Log($"Stacking {item.itemName} in existing slot.");
                slot.AddItem(item, amount);
                AutoSave(); // Lưu tự động
                return;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                Debug.Log($"Adding {item.itemName} to empty inventory slot.");
                slot.AddItem(item, amount);
                AutoSave(); // Lưu tự động
                return;
            }
        }

        Debug.Log("Inventory full! No space for " + item.itemName);
    }

    public void RemoveItemFromInventory(InventoryItem item, int amount = 1)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.quantity -= amount;
                if (slot.quantity <= 0)
                    slot.ClearSlot();
                else
                    slot.quantityText.text = slot.quantity.ToString();

                AutoSave(); // Lưu tự động
                return;
            }
        }
    }

    public void RemoveItemFromHotbar(InventoryItem item)
    {
        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.ClearSlot();
                AutoSave(); // Lưu tự động
                return;
            }
        }
    }

    public void UseItemFromHotbar(int index)
    {
        if (index >= hotbarSlots.Length) return;
        var slot = hotbarSlots[index];
        if (slot == null || slot == null || slot.gameObject == null) return;
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

        AutoSave(); // Lưu tự động
    }
}