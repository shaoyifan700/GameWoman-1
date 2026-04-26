using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI currentStaminaText;

    void Start()
    {
        RefreshStamina();
    }

    void RefreshStamina()
    {
        if (currentStaminaText != null && StaminaManager.Instance != null)
            currentStaminaText.text = "当前体力：" + StaminaManager.Instance.GetStamina();
    }

    // 购买10体力
    public void OnClickBuy10()
    {
        StaminaManager.Instance.AddStamina(10);
        RefreshStamina();
    }

    // 购买30体力
    public void OnClickBuy30()
    {
        StaminaManager.Instance.AddStamina(30);
        RefreshStamina();
    }

    // 购买60体力
    public void OnClickBuy60()
    {
        StaminaManager.Instance.AddStamina(60);
        RefreshStamina();
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("ChatScene");
    }
}