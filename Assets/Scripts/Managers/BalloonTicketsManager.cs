using UnityEngine;

/// <summary>
/// 管理打气球游戏的可玩次数。
/// 新用户初始 1 次，每玩一局消耗 1 次，可在商店购买。
/// </summary>
public class BalloonTicketsManager : MonoBehaviour
{
    public static BalloonTicketsManager Instance;

    private const string KEY = "BalloonTickets_";
    private const int INITIAL_TICKETS = 2;

    private int currentTickets;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void InitTickets(string userName)
    {
        string key = KEY + userName;
        if (!PlayerPrefs.HasKey(key))
            PlayerPrefs.SetInt(key, INITIAL_TICKETS);
        currentTickets = PlayerPrefs.GetInt(key, INITIAL_TICKETS);
    }

    public int GetTickets() { return currentTickets; }

    /// <summary>消耗一次。成功返回 true；次数不足返回 false。</summary>
    public bool UseTicket()
    {
        if (currentTickets <= 0) return false;
        currentTickets--;
        Save();
        return true;
    }

    public void AddTickets(int count)
    {
        currentTickets += count;
        Save();
    }

    void Save()
    {
        string user = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "default";
        PlayerPrefs.SetInt(KEY + user, currentTickets);
        PlayerPrefs.Save();
    }
}
