using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    public Image healthFill;
    public Image staminaFill;
    public Image manaFill;

    public void UpdateHealth(float value)
    {
        if (healthFill == null)
        {
            Debug.LogWarning("Health slider is null, skipping UpdateHealth.");
            return;
        }
        //healthFill.fillAmount = value;
    }

    public void UpdateStamina(float value)
    {
      
        if (staminaFill == null)
        {
            Debug.LogWarning("Stamina slider is null, skipping UpdateStamina.");
            return;
        }
        //staminaFill.fillAmount = value;
    }

    public void UpdateMana(float value)
    {
        if (manaFill == null)
        {
            Debug.LogWarning("Mana slider is null, skipping UpdateMana.");
            return;
        }

        //manaFill.fillAmount = value;
    }
}
