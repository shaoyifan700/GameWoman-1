using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 成就解锁弹窗。屏幕中央显示，淡入+缩放进入，淡出退出，总共4秒。
/// 玩家点击"跳过"按钮可立即关闭。
/// </summary>
public class AchievementToast : MonoBehaviour
{
    [Header("UI组件")]
    public CanvasGroup canvasGroup;          // 用于淡入淡出
    public RectTransform contentRect;         // 用于缩放进入
    public TextMeshProUGUI titleText;        // "🏆 成就解锁"
    public TextMeshProUGUI nameText;          // 成就名
    public TextMeshProUGUI descText;          // 成就描述
    public Button btnSkip;                    // 跳过按钮

    [Header("动画参数（总时长 4 秒）")]
    public float fadeInDuration  = 0.6f;     // 淡入时长
    public float displayDuration = 2.8f;     // 显示停留
    public float fadeOutDuration = 0.6f;     // 淡出时长

    private Coroutine currentRoutine;
    private bool subscribed = false;

    void Awake()
    {
        if (btnSkip != null) btnSkip.onClick.AddListener(SkipNow);

        // 初始隐藏：用 CanvasGroup 隐藏，不能 SetActive(false)，否则脚本不再响应事件
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        TrySubscribe();
    }

    void Start()
    {
        // 二次保险：万一 AchievementManager 在 Awake 时还没初始化好，这里再订阅一次
        TrySubscribe();
    }

    void TrySubscribe()
    {
        if (subscribed) return;
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OnAchievementUnlocked += ShowAchievement;
            subscribed = true;
            Debug.Log("AchievementToast 已订阅成就解锁事件");
        }
    }

    void OnDestroy()
    {
        if (subscribed && AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= ShowAchievement;
    }

    public void ShowAchievement(AchievementDef def)
    {
        if (def == null) return;
        Debug.Log("Toast 收到解锁事件: " + def.name);

        if (titleText != null) titleText.text = "🏆 成就解锁";
        if (nameText != null)  nameText.text  = def.name;
        if (descText != null)  descText.text  = def.description;

        // 显示并允许交互
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // 初始状态
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (contentRect != null) contentRect.localScale = new Vector3(0.7f, 0.7f, 1f);

        // 阶段1：淡入 + 缩放
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeInDuration);
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            if (canvasGroup != null) canvasGroup.alpha = ease;
            if (contentRect != null) contentRect.localScale = Vector3.Lerp(
                new Vector3(0.7f, 0.7f, 1f), Vector3.one, ease);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (contentRect != null) contentRect.localScale = Vector3.one;

        // 阶段2：稳定显示
        yield return new WaitForSecondsRealtime(displayDuration);

        // 阶段3：淡出
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            if (canvasGroup != null) canvasGroup.alpha = 1f - p;
            yield return null;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        currentRoutine = null;
    }

    /// <summary>跳过按钮点击：立即关闭弹窗。</summary>
    public void SkipNow()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        currentRoutine = null;
    }
}
