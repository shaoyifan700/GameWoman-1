using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HomePageManager : MonoBehaviour
{
    public AudioSource bgmAudioSource;
    public Slider volumeSlider;

    [Header("成就系统")]
    public Button btnAchievements;                       // "成就"按钮
    public AchievementListPanel achievementListPanel;    // 成就列表面板

    [Header("钱包")]
    public TextMeshProUGUI walletText;                   // 钱包余额显示

    [Header("射击次数")]
    public TextMeshProUGUI ticketsText;                  // 剩余打气球次数显示
    public GameObject noTicketsPanel;                    // 次数不足提示面板
    public Button btnCloseNoTickets;                     // 关闭提示按钮

    void Start()
    {

        // 读取上次保存的音量，默认0.5
        float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        volumeSlider.value = savedVolume;
        bgmAudioSource.volume = savedVolume;

        // 监听滑动条变化
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // 成就按钮
        if (btnAchievements != null && achievementListPanel != null)
            btnAchievements.onClick.AddListener(() => achievementListPanel.Show());

        // 次数不足提示面板默认隐藏
        if (noTicketsPanel != null) noTicketsPanel.SetActive(false);
        if (btnCloseNoTickets != null) btnCloseNoTickets.onClick.AddListener(OnClickCloseNoTickets);

        // 刷新钱包显示
        RefreshWallet();
    }

    void RefreshWallet()
    {
        if (walletText != null && WalletManager.Instance != null)
            walletText.text = "" + WalletManager.Instance.GetBalance().ToString("0");

        if (ticketsText != null && BalloonTicketsManager.Instance != null)
            ticketsText.text = "剩余次数：" + BalloonTicketsManager.Instance.GetTickets();
    }

    // 跳转到打气球小游戏（先检查次数）
    public void OnClickPlayBall()
    {
        if (BalloonTicketsManager.Instance == null)
        {
            SceneManager.LoadScene("gameMoney");
            return;
        }

        int tickets = BalloonTicketsManager.Instance.GetTickets();
        if (tickets <= 0)
        {
            // 次数为0，弹提示
            if (noTicketsPanel != null)
            {
                noTicketsPanel.SetActive(true);
                noTicketsPanel.transform.SetAsLastSibling();
            }
            return;
        }

        // 扣一次再进游戏
        BalloonTicketsManager.Instance.UseTicket();
        SceneManager.LoadScene("gameMoney");
    }

    public void OnClickCloseNoTickets()
    {
        if (noTicketsPanel != null) noTicketsPanel.SetActive(false);
    }

    // 跳转到商店
    public void OnClickShop()
    {
        SceneManager.LoadScene("CartShop");
    }

    void OnVolumeChanged(float value)
    {
        bgmAudioSource.volume = value;
        // 保存音量设置
        PlayerPrefs.SetFloat("BGMVolume", value);
    }
    public void OnClickLinChenxi()
{
    ResetForNewPlaythrough(1);
    SceneManager.LoadScene("MainMenu");
}

public void OnClickGuYunshen()
{
    ResetForNewPlaythrough(2);
    SceneManager.LoadScene("GuScene");
}

public void OnClickXiaXinghe()
{
    ResetForNewPlaythrough(3);
    SceneManager.LoadScene("XiaScene");
}

// 进入新一局剧情前，清空好感度和读档标记，确保从头开始
void ResetForNewPlaythrough(int characterId)
{
    PlayerPrefs.SetInt("SelectedCharacter", characterId);

    // 清空内存中残留的好感度
    if (GameManager.Instance != null)
        GameManager.Instance.ResetAllFavorability();

    // 清空读档临时键，避免 DialogueManager 误判为读档
    PlayerPrefs.DeleteKey("LoadDialogueId");
    PlayerPrefs.DeleteKey("LoadFav1");
    PlayerPrefs.DeleteKey("LoadFav2");
    PlayerPrefs.DeleteKey("LoadFav3");
    PlayerPrefs.Save();
}

    public void OnClickQuit()
    {
        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();
    }
    public void OnClickChat()
{
    SceneManager.LoadScene("AIChatScene");
}

    public void onClickBack()
    {
        SceneManager.LoadScene("UserSelect");
    }
}