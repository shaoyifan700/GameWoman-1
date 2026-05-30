using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 成就列表面板：HomePage 上点"成就"按钮打开，显示所有成就的解锁状态。
/// </summary>
public class AchievementListPanel : MonoBehaviour
{
    [Header("容器与预制体")]
    public Transform itemContainer;     // 装成就条目的容器（带 Vertical Layout Group）
    public GameObject itemPrefab;       // 单条成就的预制体

    [Header("关闭按钮")]
    public Button btnClose;

    void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(Hide);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshList();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void RefreshList()
    {
        // 清空旧条目
        foreach (Transform t in itemContainer)
            Destroy(t.gameObject);

        if (AchievementManager.Instance == null) return;

        var defs = AchievementManager.Instance.GetAllDefinitions();
        foreach (var def in defs)
        {
            GameObject go = Instantiate(itemPrefab, itemContainer);
            SetupItem(go, def);
        }
    }

    void SetupItem(GameObject go, AchievementDef def)
    {
        // 在预制体中查找命名子物体
        TextMeshProUGUI nameText = FindChild<TextMeshProUGUI>(go, "NameText");
        TextMeshProUGUI descText = FindChild<TextMeshProUGUI>(go, "DescText");
        Image            iconImg  = FindChild<Image>(go, "StatusIcon");

        bool unlocked = AchievementManager.Instance.IsUnlocked(def.id);

        if (nameText != null) nameText.text = def.name;
        if (descText != null)
        {
            if (def.isHidden && !unlocked)
                descText.text = "??? 隐藏成就 ???";
            else
                descText.text = def.description;
        }

        // 未达成时，在item上覆盖半透明黑色遮罩；达成则隐藏遮罩
        Transform overlayT = go.transform.Find("LockOverlay");
        GameObject overlay;

        if (overlayT == null)
        {
            // 动态创建遮罩
            overlay = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            overlay.transform.SetParent(go.transform, false);

            // 关键：让遮罩脱离父物体的 Horizontal/Vertical Layout Group
            LayoutElement le = overlay.GetComponent<LayoutElement>();
            le.ignoreLayout = true;

            // 铺满父物体
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image img = overlay.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);    // 半透明黑色
            img.raycastTarget = false;                   // 不阻挡点击
        }
        else
        {
            overlay = overlayT.gameObject;
        }

        // 遮罩始终在最上层（盖住所有其他子物体）
        overlay.transform.SetAsLastSibling();
        overlay.SetActive(!unlocked);
    }

    static T FindChild<T>(GameObject root, string childName) where T : Component
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName) return t.GetComponent<T>();
        }
        return null;
    }
}
