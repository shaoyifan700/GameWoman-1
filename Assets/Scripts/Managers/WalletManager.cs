using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance;

    private const string WALLET_KEY = "Wallet_";
    private const float INITIAL_BALANCE = 20f;
    private float currentBalance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitWallet(string userName)
    {
        string key = WALLET_KEY + userName;
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetFloat(key, INITIAL_BALANCE);
        currentBalance = PlayerPrefs.GetFloat(key, INITIAL_BALANCE);
    }

    public float GetBalance()
    {
        return currentBalance;
    }

    public bool Spend(float amount)
    {
        if (currentBalance < amount) return false;
        currentBalance -= amount;
        SaveWallet();
        return true;
    }

    public void AddMoney(float amount)
    {
        currentBalance += amount;
        SaveWallet();
    }

    void SaveWallet()
    {
        string userName = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "default";
        PlayerPrefs.SetFloat(WALLET_KEY + userName, currentBalance);
        PlayerPrefs.Save();
    }
}