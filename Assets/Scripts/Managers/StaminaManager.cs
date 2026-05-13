using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance;

    private int maxStamina = 9;
    private int currentStamina;
    private const string STAMINA_KEY = "Stamina_";

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

    // 初始化当前用户的体力
    public void InitStamina(string userName)
    {
        string key = STAMINA_KEY + userName;
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, maxStamina);
        currentStamina = PlayerPrefs.GetInt(key, maxStamina);
    }

    public int GetStamina()
    {
        return currentStamina;
    }

    // 消耗体力
    public bool UseStamina(int amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        SaveStamina();
        return true;
    }

    // 充值体力
    public void AddStamina(int amount)
    {
        currentStamina += amount;
        SaveStamina();
    }

    void SaveStamina()
    {
        string userName = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "default";
        PlayerPrefs.SetInt(STAMINA_KEY + userName, currentStamina);
        PlayerPrefs.Save();
    }
}