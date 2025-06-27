using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    public Image healthFill;
    public Image staminaFill;
    public Image manaFill;

    public void UpdateHealth(float value)
    {
        healthFill.fillAmount = value;
    }

    public void UpdateStamina(float value)
    {
        staminaFill.fillAmount = value;
    }

    public void UpdateMana(float value)
    {
        manaFill.fillAmount = value;
    }
}
