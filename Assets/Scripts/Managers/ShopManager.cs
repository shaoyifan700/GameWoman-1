using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI currentStaminaText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI tipText;

    private float price10 = 6f;
    private float price30 = 15f;
    private float price60 = 25f;

    void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (currentStaminaText != null && StaminaManager.Instance != null)
            currentStaminaText.text = "当前体力：" + StaminaManager.Instance.GetStamina();

        if (balanceText != null && WalletManager.Instance != null)
            balanceText.text = "账户余额：¥" + WalletManager.Instance.GetBalance().ToString("F2");
    }

    void TryBuy(float price, int staminaAmount)
    {
        if (WalletManager.Instance == null || StaminaManager.Instance == null) return;

        if (!WalletManager.Instance.Spend(price))
        {
            ShowTip("余额不足，无法购买！");
            return;
        }

        StaminaManager.Instance.AddStamina(staminaAmount);
        RefreshUI();
        ShowTip("购买成功！获得 " + staminaAmount + " 体力");
    }

    public void OnClickBuy10() => TryBuy(price10, 10);
    public void OnClickBuy30() => TryBuy(price30, 30);
    public void OnClickBuy60() => TryBuy(price60, 60);

    void ShowTip(string msg)
    {
        if (tipText != null) tipText.text = msg;
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("AIChatScene");
    }
}
