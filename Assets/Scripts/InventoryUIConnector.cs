using UnityEngine;

public class InventoryUIConnector : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUIPanel;

    void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.inventoryUI = inventoryUIPanel;
            // Optionally, set the inventory UI to inactive by default when the scene loads
            if (inventoryUIPanel != null)
            {
                inventoryUIPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("InventoryManager.Instance is null. Make sure InventoryManager is in the scene and set to DontDestroyOnLoad.");
        }
    }
}