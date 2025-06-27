using UnityEngine;

public class PotionInteraction : MonoBehaviour
{
    public GameObject pressEText;
    private bool isPlayerNear = false;

    void Start()
    {
        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            PickUpPotion();
        }
    }

    void PickUpPotion()
    {
        Debug.Log("Potion picked up!");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player gần potion");
            isPlayerNear = true;
            if (pressEText != null)
            {
                Debug.Log("Có gán PressEText");
                pressEText.SetActive(true);
            }
            else
            {
                Debug.Log("PressEText null");
            }
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (pressEText != null)
                pressEText.SetActive(false);
        }
    }
}
