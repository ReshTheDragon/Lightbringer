using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public InventoryItem currentItem;
    public int quantity;
    public int slotIndex;

    public const int MaxStack = 5;

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.hoveredSlot = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.hoveredSlot = null;
    }

    private void Awake()
    {
        if (icon == null)
        {
            icon = GetComponentInChildren<Image>();
            if (icon == null)
                Debug.LogError($"Slot {gameObject.name} không tìm thấy Image con!");
        }
        quantityText = GetComponentInChildren<TextMeshProUGUI>();
        if (quantityText == null)
            Debug.LogError($"Slot {gameObject.name} không tìm thấy Text con cho quantity!");
        else
            quantityText.enabled = false;

        Debug.Log($"InventorySlot Awake: {gameObject.name}, Icon: {icon != null}, QuantityText: {quantityText != null}");
    }

    public void AddItem(InventoryItem newItem, int amount = 1)
    {
        Debug.Log($"AddItem called for {gameObject.name} with item {newItem.itemName}, amount {amount}");

        if (currentItem != null && currentItem == newItem)
        {
            int availableSpace = MaxStack - quantity;
            int amountToAdd = Mathf.Min(availableSpace, amount);
            quantity += amountToAdd;

            Debug.Log($"Stacking item. Added {amountToAdd}. New quantity: {quantity}");

            if (amountToAdd < amount)
                Debug.Log($"Slot {gameObject.name} đầy, còn dư {amount - amountToAdd} item.");
        }
        else
        {
            currentItem = newItem;
            quantity = Mathf.Min(amount, MaxStack);
            Debug.Log($"Adding new item. Quantity: {quantity}");

            if (amount > MaxStack)
                Debug.Log($"Slot {gameObject.name} đầy, còn dư {amount - MaxStack} item.");
        }

        if (icon != null)
        {
            icon.sprite = newItem.icon;
            icon.enabled = true;
            icon.color = new Color(1, 1, 1, 1);
            Debug.Log($"Icon updated for {gameObject.name}");
        }
        else
        {
            Debug.LogError("Icon is null on " + gameObject.name);
        }

        UpdateQuantityText();
    }

    public void ClearSlot()
    {
        Debug.Log($"ClearSlot called for {gameObject.name}");
        currentItem = null;
        quantity = 0;

        if (icon != null)
        {
            icon.sprite = null;
            icon.color = new Color(1, 1, 1, 0);
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }
    }

    public void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            if (currentItem != null && quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
            Debug.Log($"Quantity text updated for {gameObject.name}: {quantityText.text}");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DragAndDrop dragAndDrop = droppedObject.GetComponent<DragAndDrop>();
        if (dragAndDrop != null)
        {
            InventorySlot originalSlot = droppedObject.GetComponentInParent<InventorySlot>();
            if (originalSlot != null && originalSlot != this)
            {
                // Swap items
                InventoryItem tempItem = currentItem;
                int tempQuantity = quantity;

                AddItem(originalSlot.currentItem, originalSlot.quantity);

                if (tempItem != null)
                {
                    originalSlot.AddItem(tempItem, tempQuantity);
                }
                else
                {
                    originalSlot.ClearSlot();
                }
            }
        }
    }
}
