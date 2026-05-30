using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// 单个气球：自下而上飘动，被点击时爆裂并加分。
/// </summary>
public class Balloon : MonoBehaviour, IPointerClickHandler
{
    [Header("移动")]
    public float floatSpeed = 200f;       // 上升速度（像素/秒）
    public float horizontalSway = 30f;    // 左右摆动幅度
    public float swaySpeed = 2f;          // 摆动速度

    [Header("UI 子物体")]
    public TextMeshProUGUI valueText;     // 显示数字
    public Image bodyImage;               // 气球本体（用于变色/爆裂动画）

    private int value;
    private RectTransform rt;
    private float startX;
    private float lifeTime;
    private bool popped = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        startX = rt.anchoredPosition.x;
    }

    public void SetValue(int v)
    {
        value = v;
        if (valueText != null) valueText.text = v.ToString();

        // 根据分值加载对应颜色的气球图（从 Resources/Balloons/）
        if (bodyImage != null)
        {
            Sprite sp = Resources.Load<Sprite>("Balloons/Balloon_" + v);
            if (sp != null) bodyImage.sprite = sp;
        }

        // 根据分值调整上升速度：数值越大，速度越快（更难打中）
        switch (v)
        {
            case 1: floatSpeed = 200f; break;  // 红 - 最慢
            case 3: floatSpeed = 270f; break;  // 橙
            case 5: floatSpeed = 350f; break;  // 黄
            case 7: floatSpeed = 420f; break;  // 蓝
            case 9: floatSpeed = 530f; break;  // 紫 - 最快
            default: floatSpeed = 200f; break;
        }
    }

    void Update()
    {
        if (popped) return;

        lifeTime += Time.deltaTime;

        // 向上飘 + 左右摆
        Vector2 pos = rt.anchoredPosition;
        pos.y += floatSpeed * Time.deltaTime;
        pos.x = startX + Mathf.Sin(lifeTime * swaySpeed) * horizontalSway;
        rt.anchoredPosition = pos;

        // 飘到屏幕外销毁
        if (pos.y > 800f) Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (popped) return;
        Pop();
    }

    void Pop()
    {
        popped = true;

        // 通知 GameManager 加分
        if (BalloonGameManager.Instance != null)
            BalloonGameManager.Instance.OnBalloonPopped(value);

        // 播放爆裂动画后销毁
        StartCoroutine(PopEffect());
    }

    IEnumerator PopEffect()
    {
        // 简单爆裂效果：快速放大 + 淡出
        float t = 0f;
        float duration = 0.25f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.5f;

        CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, p);
            cg.alpha = 1f - p;
            yield return null;
        }

        Destroy(gameObject);
    }
}
