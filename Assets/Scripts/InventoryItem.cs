using UnityEngine;

public enum ItemType
{
    ManaPotion,
    StaminaPotion,
    HealthPotion,
    Other
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
}
