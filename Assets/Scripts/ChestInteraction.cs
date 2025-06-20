using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    public Sprite openedChestSprite;
    private bool isPlayerNear = false;
    private SpriteRenderer spriteRenderer;
    private bool isOpened = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
