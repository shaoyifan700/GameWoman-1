using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 5槽位存档/读档面板。
/// 模式：SAVE = 点空槽直接存，点已有槽位提示覆盖确认
///       LOAD = 只能点已有槽位，空槽灰显
/// </summary>
public class SaveSlotsPanel : MonoBehaviour
{
    public enum Mode { Save, Load }

    [Header("槽位预制体与容器")]
    public Transform slotContainer;        // 装5个槽位按钮的容器
    public GameObject slotPrefab;          // 单个槽位的预制体

    [Header("通用UI")]
    public Button btnClose;
    public TextMeshProUGUI titleText;      // "选择存档" / "选择读档"

    [Header("确认弹窗")]
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public Button btnConfirmYes;
    public Button btnConfirmNo;

    private Mode currentMode = Mode.Save;
    private int pendingSlotIndex = -1;
    private System.Action pendingAction;

    void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(Hide);
        if (btnConfirmYes != null) btnConfirmYes.onClick.AddListener(OnConfirmYes);
        if (btnConfirmNo  != null) btnConfirmNo.onClick.AddListener(OnConfirmNo);

        if (confirmPanel != null) confirmPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Show(Mode mode)
    {
        currentMode = mode;
        if (titleText != null)
            titleText.text = mode == Mode.Save ? "选择存档位置" : "选择读档位置";

        RefreshSlots();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void RefreshSlots()
    {
        // 清空旧槽位
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        SaveSlot[] slots = SaveManager.Instance.GetAllSlots();
        foreach (SaveSlot s in slots)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            SetupSlotUI(go, s);
        }
    }

    void SetupSlotUI(GameObject go, SaveSlot slot)
    {
        // 查找子物体（名字要严格匹配预制体里的命名）
        TextMeshProUGUI slotIndexText = FindChild<TextMeshProUGUI>(go, "SlotIndexText");
        TextMeshProUGUI infoText      = FindChild<TextMeshProUGUI>(go, "InfoText");
        TextMeshProUGUI timeText      = FindChild<TextMeshProUGUI>(go, "TimeText");

        if (slotIndexText != null) slotIndexText.text = "槽位 " + slot.slotIndex;

        if (slot.isEmpty)
        {
            if (infoText != null) infoText.text = "-- 空 --";
            if (timeText != null) timeText.text = "";
        }
        else
        {
            if (infoText != null) infoText.text = slot.characterName + " · 对话 " + slot.currentDialogueId;
            if (timeText != null) timeText.text = slot.timestamp;
        }

        Button btn = go.GetComponent<Button>();
        if (btn != null)
        {
            int capturedIdx = slot.slotIndex;
            bool capturedEmpty = slot.isEmpty;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSlotClicked(capturedIdx, capturedEmpty));

            // 读档模式下空槽灰显
            if (currentMode == Mode.Load && capturedEmpty)
                btn.interactable = false;
            else
                btn.interactable = true;
        }
    }

    void OnSlotClicked(int slotIndex, bool isEmpty)
    {
        if (currentMode == Mode.Save)
        {
            if (isEmpty)
            {
                // 空槽直接存
                DoSave(slotIndex);
            }
            else
            {
                // 已有存档，弹覆盖确认
                pendingSlotIndex = slotIndex;
                pendingAction = () => DoSave(slotIndex);
                ShowConfirm("槽位 " + slotIndex + " 已有存档，确认覆盖吗？");
            }
        }
        else // Load
        {
            if (isEmpty) return;
            pendingSlotIndex = slotIndex;
            pendingAction = () => DoLoad(slotIndex);
            ShowConfirm("读取槽位 " + slotIndex + " 的存档？\n（当前未保存的进度将丢失）");
        }
    }

    void ShowConfirm(string msg)
    {
        if (confirmPanel == null || confirmText == null) return;
        confirmText.text = msg;
        confirmPanel.SetActive(true);
        confirmPanel.transform.SetAsLastSibling();
    }

    void OnConfirmYes()
    {
        confirmPanel.SetActive(false);
        if (pendingAction != null) pendingAction.Invoke();
        pendingAction = null;
        pendingSlotIndex = -1;
    }

    void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
        pendingAction = null;
        pendingSlotIndex = -1;
    }

    void DoSave(int slotIndex)
    {
        int dialogueId = 0;
        DialogueManager dm = FindObjectOfType<DialogueManager>();
        if (dm != null) dialogueId = dm.GetCurrentDialogueId();

        string sceneName = SceneManager.GetActiveScene().name;
        SaveManager.Instance.SaveToSlot(slotIndex, dialogueId, sceneName);
        RefreshSlots(); // 刷新显示
    }

    void DoLoad(int slotIndex)
    {
        SaveSlot s = SaveManager.Instance.LoadFromSlot(slotIndex);
        if (s == null) return;

        // 把读档信息写入 PlayerPrefs，等场景加载时 DialogueManager 读
        PlayerPrefs.SetInt("SelectedCharacter", s.selectedCharacter);
        PlayerPrefs.SetInt("LoadDialogueId", s.currentDialogueId);
        PlayerPrefs.SetInt("LoadFav1", s.favorability1);
        PlayerPrefs.SetInt("LoadFav2", s.favorability2);
        PlayerPrefs.SetInt("LoadFav3", s.favorability3);
        PlayerPrefs.Save();

        Time.timeScale = 1;
        SceneManager.LoadScene(s.sceneName);
    }

    // 递归在子物体里按名字找组件
    static T FindChild<T>(GameObject root, string childName) where T : Component
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName) return t.GetComponent<T>();
        }
        return null;
    }
}
