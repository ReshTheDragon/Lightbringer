using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

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
            Debug.Log($"InventoryManager initialized. Save path: {savePath}");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate InventoryManager detected, destroying this instance.");
        }
    }

    void Start()
    {
        // Khởi tạo inventorySlots và hotbarSlots nếu chưa có
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogWarning("Inventory slots not assigned. Attempting to find them.");
            RefreshSlotReferences();
        }
        if (hotbarSlots == null || hotbarSlots.Length == 0)
        {
            Debug.LogWarning("Hotbar slots not assigned. Attempting to find them.");
            RefreshSlotReferences();
        }

        ClearAllSlots();
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);
        LoadInventoryFromJSON();
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            Debug.Log("Inventory UI activated on start.");
        }
    }

    public void SaveInventoryToJSON()
    {
        InventoryData data = new InventoryData();

        // Kiểm tra và khởi tạo mảng nếu null
        if (hotbarSlots == null) hotbarSlots = new InventorySlot[0];
        if (inventorySlots == null) inventorySlots = new InventorySlot[0];

        // Lưu hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null && hotbarSlots[i].currentItem != null)
            {
                data.hotbarSlots.Add(new SlotData(hotbarSlots[i].currentItem.itemName, hotbarSlots[i].quantity));
                Debug.Log($"Saving hotbar slot {i}: {hotbarSlots[i].currentItem.itemName}, qty: {hotbarSlots[i].quantity}");
            }
            else
            {
                data.hotbarSlots.Add(new SlotData("", 0));
                Debug.Log($"Saving hotbar slot {i}: Empty");
            }
        }

        // Lưu inventory
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].currentItem != null)
            {
                data.inventorySlots.Add(new SlotData(inventorySlots[i].currentItem.itemName, inventorySlots[i].quantity));
                Debug.Log($"Saving inventory slot {i}: {inventorySlots[i].currentItem.itemName}, qty: {inventorySlots[i].quantity}");
            }
            else
            {
                data.inventorySlots.Add(new SlotData("", 0));
                Debug.Log($"Saving inventory slot {i}: Empty");
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Inventory saved to JSON: {savePath}\nJSON content: {json}");
    }

    public void LoadInventoryFromJSON()
    {
        if (File.Exists(savePath))
        {
            Debug.Log($"Loading inventory from: {savePath}");
            string json = File.ReadAllText(savePath);
            Debug.Log($"JSON content: {json}");

            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            if (data == null)
            {
                Debug.LogError("Failed to deserialize JSON data!");
                return;
            }

            StartCoroutine(LoadInventoryCoroutine(data));
        }
        else
        {
            Debug.Log("No save file found. Starting with empty inventory.");
        }
    }

    private IEnumerator LoadInventoryCoroutine(InventoryData data)
    {
        yield return new WaitForEndOfFrame();

        RefreshSlotReferences();
        Debug.Log($"Hotbar slots count: {hotbarSlots.Length}, Inventory slots count: {inventorySlots.Length}");
        Debug.Log($"Data hotbar slots: {data.hotbarSlots.Count}, Data inventory slots: {data.inventorySlots.Count}");

        // Tải hotbar
        for (int i = 0; i < data.hotbarSlots.Count && i < hotbarSlots.Length; i++)
        {
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
                    hotbarSlots[i].AddItem(item, data.hotbarSlots[i].quantity);
                }
                else
                {
                    Debug.LogWarning($"Item not found: {data.hotbarSlots[i].itemName}");
                }
            }
        }

        // Tải inventory
        for (int i = 0; i < data.inventorySlots.Count && i < inventorySlots.Length; i++)
        {
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
                    inventorySlots[i].AddItem(item, data.inventorySlots[i].quantity);
                }
                else
                {
                    Debug.LogWarning($"Item not found: {data.inventorySlots[i].itemName}");
                }
            }
        }

        Debug.Log("Inventory loaded from JSON!");
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            Debug.Log("Inventory UI activated after load.");
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted!");
        }
    }

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
        Debug.LogWarning("InventoryManager is being destroyed!");
        SaveInventoryToJSON();
    }

    private void AutoSave()
    {
        SaveInventoryToJSON();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedLoadAfterSceneChange());
    }

    private IEnumerator DelayedLoadAfterSceneChange()
    {
        float maxWaitTime = 5f;
        float waitTime = 0f;

        // Tìm InventoryPanel
        while (inventoryUI == null && waitTime < maxWaitTime)
        {
            inventoryUI = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
            waitTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (inventoryUI == null)
        {
            Debug.LogError("Failed to find InventoryPanel after scene load!");
            yield break;
        }

        Debug.Log($"Found InventoryPanel: {inventoryUI.name}, Active: {inventoryUI.activeSelf}");

        RefreshSlotReferences();
        if (inventorySlots.Length == 0 || hotbarSlots.Length == 0)
        {
            Debug.LogError($"No inventory or hotbar slots found! InventoryPanel: {inventoryUI.name}, Children: {inventoryUI.transform.childCount}");
            yield break;
        }

        LoadInventoryFromJSON();
    }

    private void RefreshSlotReferences()
    {
        // Tìm InventoryPanel
        GameObject inventoryPanel = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
        if (inventoryPanel != null)
        {
            inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>(true);
            Debug.Log($"Found InventoryPanel: {inventoryPanel.name}, Slots found: {inventorySlots.Length}");
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                Debug.Log($"Inventory slot {i}: {(inventorySlots[i] != null ? inventorySlots[i].gameObject.name : "null")}");
            }
        }
        else
        {
            inventorySlots = new InventorySlot[0];
            Debug.LogError("InventoryPanel not found, setting inventorySlots to empty array.");
        }

        // Tìm HotbarPanel
        GameObject hotbarPanel = GameObject.Find("HotbarPanel") ?? GameObject.Find("Panel/HotbarPanel") ?? GameObject.FindWithTag("HotbarPanel");
        if (hotbarPanel != null)
        {
            hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>(true);
            Debug.Log($"Found HotbarPanel: {hotbarPanel.name}, Slots found: {hotbarSlots.Length}");
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                Debug.Log($"Hotbar slot {i}: {(hotbarSlots[i] != null ? hotbarSlots[i].gameObject.name : "null")}");
            }
        }
        else
        {
            hotbarSlots = new InventorySlot[0];
            Debug.LogError("HotbarPanel not found, setting hotbarSlots to empty array.");
        }
    }

    public void ClearAllSlots()
    {
        if (hotbarSlots != null)
        {
            foreach (var slot in hotbarSlots)
            {
                if (slot != null && slot.gameObject != null)
                    slot.ClearSlot();
            }
        }

        if (inventorySlots != null)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot != null && slot.gameObject != null)
                    slot.ClearSlot();
            }
        }
    }

    public static bool isInventoryOpen = false;

    private void Update()
    {
        if (inventoryUI == null)
        {
            inventoryUI = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
            if (inventoryUI == null)
            {
                Debug.LogError("InventoryUI not found!");
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

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveInventoryToJSON();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadInventoryFromJSON();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            InventoryItem testItem = FindItemByName("Health");
            Debug.Log(testItem != null ? $"Found Health item: {testItem.name}" : "Health item not found!");
        }

        if (Input.GetKeyDown(KeyCode.F8)) // Thêm phím để xóa file JSON
        {
            DeleteSaveFile();
        }
    }

    private InventoryItem FindItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogError("FindItemByName called with null or empty itemName!");
            return null;
        }

        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
        Debug.Log($"Searching for item: {itemName}. Found {allItems.Length} items in Resources/Items");

        foreach (InventoryItem item in allItems)
        {
            if (item == null)
            {
                Debug.LogWarning("Found null item in Resources/Items!");
                continue;
            }
            if (item.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Found matching item: {item.itemName}");
                return item;
            }
        }

        Debug.LogWarning($"Item not found in Resources: {itemName}");
        return null;
    }

    void SwapHoveredItemWithHotbar(int hotbarIndex)
    {
        if (hoveredSlot == null || System.Array.IndexOf(inventorySlots, hoveredSlot) == -1) return;
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

        AutoSave();
    }

    public bool AddItemToHotbar(InventoryItem item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("AddItemToHotbar called with null item!");
            return false;
        }

        if (hotbarSlots == null || hotbarSlots.Length == 0)
        {
            Debug.LogError("Hotbar slots not initialized! Attempting to refresh.");
            RefreshSlotReferences();
            if (hotbarSlots.Length == 0) return false;
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.AddItem(item, amount);
                AutoSave();
                Debug.Log($"Added {amount} {item.itemName} to existing hotbar slot.");
                return true;
            }
        }

        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                slot.AddItem(item, amount);
                AutoSave();
                Debug.Log($"Added {amount} {item.itemName} to empty hotbar slot.");
                return true;
            }
        }

        Debug.Log($"Hotbar full! Cannot add {item.itemName}.");
        return false;
    }

    public void AddItemToInventory(InventoryItem item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("AddItemToInventory called with null item!");
            return;
        }

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("Inventory slots not initialized! Attempting to refresh.");
            RefreshSlotReferences();
            if (inventorySlots.Length == 0)
            {
                Debug.LogError("No inventory slots available to add item!");
                return;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.AddItem(item, amount);
                AutoSave();
                Debug.Log($"Stacked {amount} {item.itemName} in existing inventory slot.");
                return;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == null)
            {
                slot.AddItem(item, amount);
                AutoSave();
                Debug.Log($"Added {amount} {item.itemName} to empty inventory slot.");
                return;
            }
        }

        Debug.Log($"Inventory full! Cannot add {item.itemName}.");
    }

    public void RemoveItemFromInventory(InventoryItem item, int amount = 1)
    {
        if (item == null) return;

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

                AutoSave();
                return;
            }
        }
    }

    public void RemoveItemFromHotbar(InventoryItem item)
    {
        if (item == null) return;

        foreach (var slot in hotbarSlots)
        {
            if (slot == null || slot.gameObject == null) continue;

            if (slot.currentItem == item)
            {
                slot.ClearSlot();
                AutoSave();
                return;
            }
        }
    }

    public void UseItemFromHotbar(int index)
    {
        if (index >= hotbarSlots.Length) return;
        var slot = hotbarSlots[index];
        if (slot == null || slot.gameObject == null || slot.currentItem == null) return;

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
                Debug.Log($"Item {item.itemName} cannot be used.");
                break;
        }

        if (slot.quantity <= 0)
            slot.ClearSlot();
        else
            slot.quantityText.text = slot.quantity.ToString();

        AutoSave();
    }
}