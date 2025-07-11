using UnityEngine;
public class ChestInteraction : MonoBehaviour
{
    public Sprite openedChestSprite;
    public InventoryItem[] itemsToDrop;
    public Vector3 spawnOffset = Vector3.zero;
    public GameObject pressEText; // Thêm biến tham chiếu chữ E
    private bool isPlayerNear = false;
    private SpriteRenderer spriteRenderer;
    private bool isOpened = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (pressEText != null)
        {
            pressEText.SetActive(false); // Tắt khi start
        }
    }
    void Update()
    {
        if (isPlayerNear && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }
    void OpenChest()
    {
        spriteRenderer.sprite = openedChestSprite;
        isOpened = true;
        Debug.Log("Chest opened!");
        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }
        if (itemsToDrop != null && itemsToDrop.Length > 0)
        {
            foreach (InventoryItem item in itemsToDrop)
            {
                GameObject droppedItem = new GameObject(item.itemName);
                droppedItem.transform.position = transform.position + spawnOffset;

                // Set layer giống với chest
                droppedItem.layer = gameObject.layer;

                ItemPickup itemPickup = droppedItem.AddComponent<ItemPickup>();
                itemPickup.item = item;
                SpriteRenderer itemSpriteRenderer = droppedItem.AddComponent<SpriteRenderer>();
                itemSpriteRenderer.sprite = item.icon;

                // Set sorting layer giống với chest (nếu cần)
                itemSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
                itemSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder;

                // Add a Collider2D to the dropped item for interaction
                CircleCollider2D collider = droppedItem.AddComponent<CircleCollider2D>();
                collider.radius = 0.5f; // Adjust as needed
                collider.isTrigger = true; // Make it a trigger
                Rigidbody2D rb = droppedItem.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isOpened && pressEText != null)
            {
                pressEText.SetActive(true);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (pressEText != null)
            {
                pressEText.SetActive(false);
            }
        }
    }
}