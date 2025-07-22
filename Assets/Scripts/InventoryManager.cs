using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
            //Debug.Log($"InventoryManager initialized at {Time.time}. Save path: {savePath}");
        }
        else
        {
            Destroy(gameObject);
            //Debug.LogWarning($"Duplicate InventoryManager detected, destroying at {Time.time}.");
        }
    }

    void Start()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            //Debug.LogWarning($"Inventory slots not assigned at {Time.time}. Attempting to find them.");
            RefreshSlotReferences();
        }
        if (hotbarSlots == null || hotbarSlots.Length == 0)
        {
            //Debug.LogWarning($"Hotbar slots not assigned at {Time.time}. Attempting to find them.");
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
            //Debug.Log($"Inventory UI activated on start at {Time.time}.");
        }
    }

    public void SaveInventoryToJSON()
    {
        InventoryData data = new InventoryData();

        if (hotbarSlots == null) hotbarSlots = new InventorySlot[0];
        if (inventorySlots == null) inventorySlots = new InventorySlot[0];

        // Lưu hotbar
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i] != null)
            {
                string itemName = hotbarSlots[i].currentItem != null ? hotbarSlots[i].currentItem.itemName : "";
                int quantity = hotbarSlots[i].currentItem != null ? hotbarSlots[i].quantity : 0;
                data.hotbarSlots.Add(new SlotData(itemName, quantity));
                //Debug.Log($"Saving hotbar slot {i}: {itemName}, qty: {quantity} at {Time.time}.");
            }
            else
            {
                data.hotbarSlots.Add(new SlotData("", 0));
                //Debug.Log($"Saving hotbar slot {i}: Null slot at {Time.time}.");
            }
        }

        // Lưu inventory, giới hạn nghiêm ngặt bằng số slot thực tế
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                string itemName = inventorySlots[i].currentItem != null ? inventorySlots[i].currentItem.itemName : "";
                int quantity = inventorySlots[i].currentItem != null ? inventorySlots[i].quantity : 0;
                data.inventorySlots.Add(new SlotData(itemName, quantity));
                //Debug.Log($"Saving inventory slot {i}: {itemName}, qty: {quantity} at {Time.time}.");
            }
            else
            {
                data.inventorySlots.Add(new SlotData("", 0));
                //Debug.Log($"Saving inventory slot {i}: Null slot at {Time.time}.");
            }
        }

        // Kiểm tra và loại bỏ trùng lặp nếu có
        if (data.inventorySlots.Count > inventorySlots.Length)
        {
            //Debug.LogWarning($"Inventory slots count ({data.inventorySlots.Count}) exceeds assigned slots ({inventorySlots.Length}) at {Time.time}. Truncating to {inventorySlots.Length} slots.");
            data.inventorySlots.RemoveRange(inventorySlots.Length, data.inventorySlots.Count - inventorySlots.Length);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        //Debug.Log($"Inventory saved to JSON: {savePath}\nJSON content: {json} at {Time.time}.");
    }

    public void LoadInventoryFromJSON()
    {
        if (File.Exists(savePath))
        {
            //Debug.Log($"Loading inventory from: {savePath} at {Time.time}.");
            string json = File.ReadAllText(savePath);
            //Debug.Log($"JSON content: {json} at {Time.time}.");

            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            if (data == null)
            {
                //Debug.LogError($"Failed to deserialize JSON data at {Time.time}!");
                return;
            }

            StartCoroutine(LoadInventoryCoroutine(data));
        }
        else
        {
            //Debug.Log($"No save file found at {Time.time}. Starting with empty inventory.");
        }
    }

    private IEnumerator LoadInventoryCoroutine(InventoryData data)
    {
        yield return new WaitForEndOfFrame();

        RefreshSlotReferences();
        //Debug.Log($"Hotbar slots count in scene: {hotbarSlots.Length}, Inventory slots count in scene: {inventorySlots.Length} at {Time.time}.");
        //Debug.Log($"Data hotbar slots: {data.hotbarSlots.Count}, Data inventory slots: {data.inventorySlots.Count} at {Time.time}.");

        ClearAllSlots();

        // Tải hotbar, giới hạn bằng số slot thực tế
        int hotbarLimit = Mathf.Min(data.hotbarSlots.Count, hotbarSlots.Length);
        for (int i = 0; i < hotbarLimit; i++)
        {
            if (hotbarSlots[i] == null)
            {
                //Debug.LogWarning($"Hotbar slot {i} is null at {Time.time}, skipping");
                continue;
            }

            if (!string.IsNullOrEmpty(data.hotbarSlots[i].itemName) && data.hotbarSlots[i].quantity > 0)
            {
                InventoryItem item = FindItemByName(data.hotbarSlots[i].itemName);
                if (item != null)
                {
                    //Debug.Log($"Loading hotbar slot {i}: {item.itemName} x{data.hotbarSlots[i].quantity} at {Time.time}.");
                    hotbarSlots[i].AddItem(item, data.hotbarSlots[i].quantity);
                }
                else
                {
                    //Debug.LogWarning($"Item not found: {data.hotbarSlots[i].itemName} at {Time.time}.");
                }
            }
        }

        // Tải inventory, giới hạn bằng số slot thực tế
        int inventoryLimit = Mathf.Min(data.inventorySlots.Count, inventorySlots.Length);
        for (int i = 0; i < inventoryLimit; i++)
        {
            if (inventorySlots[i] == null)
            {
                //Debug.LogWarning($"Inventory slot {i} is null at {Time.time}, skipping");
                continue;
            }

            if (!string.IsNullOrEmpty(data.inventorySlots[i].itemName) && data.inventorySlots[i].quantity > 0)
            {
                InventoryItem item = FindItemByName(data.inventorySlots[i].itemName);
                if (item != null)
                {
                    //Debug.Log($"Loading inventory slot {i}: {item.itemName} x{data.inventorySlots[i].quantity} at {Time.time}.");
                    inventorySlots[i].AddItem(item, data.inventorySlots[i].quantity);
                }
                else
                {
                    //Debug.LogWarning($"Item not found: {data.inventorySlots[i].itemName} at {Time.time}.");
                }
            }
        }

        //Debug.Log($"Inventory loaded from JSON at {Time.time}.");
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            //Debug.Log($"Inventory UI activated after load at {Time.time}.");
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Save file deleted at {Time.time}!");
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
        Debug.LogWarning($"InventoryManager is being destroyed at {Time.time}!");
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

        while (inventoryUI == null && waitTime < maxWaitTime)
        {
            inventoryUI = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
            waitTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if (inventoryUI == null)
        {
            Debug.LogError($"Failed to find InventoryPanel after scene load at {Time.time}!");
            yield break;
        }

        //Debug.Log($"Found InventoryPanel: {inventoryUI.name}, Active: {inventoryUI.activeSelf} at {Time.time}.");

        RefreshSlotReferences();
        if (inventorySlots.Length == 0 || hotbarSlots.Length == 0)
        {
            Debug.LogError($"No inventory or hotbar slots found at {Time.time}! InventoryPanel: {inventoryUI.name}, Children: {inventoryUI.transform.childCount}");
            yield break;
        }

        LoadInventoryFromJSON();
    }

    private void RefreshSlotReferences()
    {
        GameObject inventoryPanel = GameObject.Find("InventoryPanel") ?? GameObject.Find("Panel/InventoryPanel") ?? GameObject.FindWithTag("InventoryUI");
        if (inventoryPanel != null)
        {
            inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>(true);
            // Lọc bỏ các slot thuộc HotbarPanel
            List<InventorySlot> validInventorySlots = new List<InventorySlot>();
            foreach (var slot in inventorySlots)
            {
                if (slot != null && slot.gameObject.transform.parent != null)
                {
                    string parentName = slot.gameObject.transform.parent.name;
                    if (!parentName.Contains("Hotbar") && !parentName.Contains("HotbarPanel"))
                    {
                        validInventorySlots.Add(slot);
                    }
                }
            }
            inventorySlots = validInventorySlots.ToArray();
            //Debug.Log($"Found InventoryPanel: {inventoryPanel.name}, Valid inventory slots: {inventorySlots.Length} at {Time.time}.");
            //for (int i = 0; i < inventorySlots.Length; i++)
            //{
            //    Debug.Log($"Inventory slot {i}: {(inventorySlots[i] != null ? inventorySlots[i].gameObject.name : "null")} at {Time.time}.");
            //}
        }
        else
        {
            inventorySlots = new InventorySlot[0];
            Debug.LogError($"InventoryPanel not found at {Time.time}, setting inventorySlots to empty array.");
        }

        GameObject hotbarPanel = GameObject.Find("HotbarPanel") ?? GameObject.Find("Panel/HotbarPanel") ?? GameObject.FindWithTag("HotbarPanel");
        if (hotbarPanel != null)
        {
            hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>(true);
            //Debug.Log($"Found HotbarPanel: {hotbarPanel.name}, Slots found: {hotbarSlots.Length} at {Time.time}.");
            //for (int i = 0; i < hotbarSlots.Length; i++)
            //{
            //    Debug.Log($"Hotbar slot {i}: {(hotbarSlots[i] != null ? hotbarSlots[i].gameObject.name : "null")} at {Time.time}.");
            //}
        }
        else
        {
            hotbarSlots = new InventorySlot[0];
            Debug.LogError($"HotbarPanel not found at {Time.time}, setting hotbarSlots to empty array.");
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
                //Debug.LogError($"InventoryUI not found at {Time.time}!");
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
            isInventoryOpen = inventoryUI.activeSelf;
            //Debug.Log($"Inventory toggled: {isInventoryOpen} at {Time.time}.");
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

        if (Input.GetKeyDown(KeyCode.F7))
        {
            InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
            Debug.Log($"Found {allItems.Length} items in Resources/Items at {Time.time}:");
            foreach (var item in allItems)
            {
                Debug.Log($"Item: {item.itemName}, Type: {item.itemType} at {Time.time}.");
            }
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            DeleteSaveFile();
        }
    }

    private InventoryItem FindItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogError($"FindItemByName called with null or empty itemName at {Time.time}!");
            return null;
        }

        InventoryItem[] allItems = Resources.LoadAll<InventoryItem>("Items");
        //Debug.Log($"Searching for item: {itemName} at {Time.time}. Found {allItems.Length} items in Resources/Items");

        foreach (InventoryItem item in allItems)
        {
            if (item == null)
            {
                Debug.LogWarning("Found null item in Resources/Items at {Time.time}!");
                continue;
            }
            //Debug.Log($"Checking item: {item.itemName} (Asset: {item.name}) vs {itemName} at {Time.time}.");
            if (item.itemName.Equals(itemName, System.StringComparison.OrdinalIgnoreCase))
            {
                //Debug.Log($"Found matching item: {item.itemName} (Asset: {item.name}) at {Time.time}.");
                return item;
            }
        }

        Debug.LogWarning($"Item not found in Resources: {itemName} at {Time.time}. Available items: {string.Join(", ", Array.ConvertAll(allItems, i => i?.itemName ?? "null"))}");
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
            Debug.LogError($"AddItemToHotbar called with null item at {Time.time}!");
            return false;
        }

        if (hotbarSlots == null || hotbarSlots.Length == 0)
        {
            Debug.LogError($"Hotbar slots not initialized at {Time.time}! Attempting to refresh.");
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
                Debug.Log($"Added {amount} {item.itemName} to existing hotbar slot at {Time.time}.");
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
                //Debug.Log($"Added {amount} {item.itemName} to empty hotbar slot at {Time.time}.");
                return true;
            }
        }

        //Debug.Log($"Hotbar full at {Time.time}! Cannot add {item.itemName}.");
        return false;
    }

    public void AddItemToInventory(InventoryItem item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError($"AddItemToInventory called with null item at {Time.time}!");
            return;
        }

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError($"Inventory slots not initialized at {Time.time}! Attempting to refresh.");
            RefreshSlotReferences();
            if (inventorySlots.Length == 0)
            {
                Debug.LogError($"No inventory slots available to add item at {Time.time}!");
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
                Debug.Log($"Stacked {amount} {item.itemName} in existing inventory slot at {Time.time}.");
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
                //Debug.Log($"Added {amount} {item.itemName} to empty inventory slot at {Time.time}.");
                return;
            }
        }

        Debug.Log($"Inventory full at {Time.time}! Cannot add {item.itemName}.");
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
                Debug.Log($"Item {item.itemName} cannot be used at {Time.time}.");
                break;
        }

        if (slot.quantity <= 0)
            slot.ClearSlot();
        else
            slot.quantityText.text = slot.quantity.ToString();

        AutoSave();
    }
}