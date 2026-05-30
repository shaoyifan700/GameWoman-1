using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 气球射击小游戏管理器。
/// 20秒倒计时，玩家点击气球获得分数，结束后分数转为金币加入钱包。
/// </summary>
public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager Instance;

    [Header("游戏参数")]
    public float gameTime = 20f;                  // 一局总时长
    public float spawnInterval = 0.1f;             // 气球生成间隔
    public int[] possibleValues = { 1, 3, 5, 7, 9 }; // 气球可能的分值
    public Vector2 spawnXRange = new Vector2(-800f, 800f); // 气球生成的X范围（Canvas坐标）

    [Header("UI 引用")]
    public RectTransform balloonsContainer;       // 气球容器（一个 RectTransform，用作生成的父物体）
    public GameObject balloonPrefab;              // 气球预制体
    public TextMeshProUGUI timerText;             // 左下角倒计时
    public TextMeshProUGUI scoreText;             // 右下角分数
    public GameObject endPanel;                   // 结算面板
    public TextMeshProUGUI endScoreText;          // 结算面板上的"获得XX分数"
    public Button btnBack;                        // 结算面板返回按钮
    public string backSceneName = "HomePage";     // 返回的场景

    private float remainingTime;
    private int score;
    private bool isPlaying;
    private float spawnTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (endPanel != null) endPanel.SetActive(false);
        if (btnBack != null) btnBack.onClick.AddListener(OnClickBack);
        StartGame();
    }

    public void StartGame()
    {
        remainingTime = gameTime;
        score = 0;
        isPlaying = true;
        spawnTimer = 0f;
        UpdateUI();
    }

    void Update()
    {
        if (!isPlaying) return;

        // 倒计时
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame();
        }

        // 定期生成气球
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnBalloon();
        }

        UpdateUI();
    }

    void SpawnBalloon()
    {
        if (balloonPrefab == null || balloonsContainer == null) return;

        GameObject go = Instantiate(balloonPrefab, balloonsContainer);
        RectTransform rt = go.GetComponent<RectTransform>();

        // 随机X位置，Y在底部往下一点
        float x = Random.Range(spawnXRange.x, spawnXRange.y);
        float y = -balloonsContainer.rect.height / 2f - 100f;  // 出生在底部外
        rt.anchoredPosition = new Vector2(x, y);

        // 随机分值
        int value = possibleValues[Random.Range(0, possibleValues.Length)];
        Balloon balloon = go.GetComponent<Balloon>();
        if (balloon != null) balloon.SetValue(value);
    }

    /// <summary>气球被点击破裂时调用，加分。</summary>
    public void OnBalloonPopped(int value)
    {
        score += value;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText != null) timerText.text = remainingTime.ToString("F1");
        if (scoreText != null) scoreText.text = score.ToString("0000");
    }

    void EndGame()
    {
        isPlaying = false;

        // 加钱到钱包
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.AddMoney(score);  // 假设有 AddMoney 方法
        }

        // 显示结算面板
        if (endPanel != null) endPanel.SetActive(true);
        if (endScoreText != null) endScoreText.text = "你获得了 " + score + " 金币！";
    }

    void OnClickBack()
    {
        SceneManager.LoadScene(backSceneName);
    }
}
