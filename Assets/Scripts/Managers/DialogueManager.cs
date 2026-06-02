using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("对话UI组件")]
    public GameObject dialoguePanel;        // 对话框面板
    public TextMeshProUGUI characterNameText; // 角色名字
    public TextMeshProUGUI dialogueText;      // 对话内容

    [Header("选项UI组件")]
    public GameObject choicePanel;          // 选项面板
    public GameObject choiceButtonPrefab;   // 选项按钮预制体
    public Transform choiceContainer;       // 选项按钮的父物体

    [Header("其他")]
    public Image backgroundImage;           // 背景图

    private Dialogue currentDialogue;
    private List<GameObject> currentChoiceButtons = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
{
    // 延迟一帧等待 GameManager 初始化完成
    StartCoroutine(StartAfterGameManager());
}



IEnumerator StartAfterGameManager()
{
    yield return null;

    int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1);

    // 优先级1：检查是否刚从槽位读档（SaveSlotsPanel.DoLoad写入的临时键）
    int loadDialogueId = PlayerPrefs.GetInt("LoadDialogueId", 0);

    if (loadDialogueId > 0)
    {
        // 从槽位读档，直接设置（而非累加）对应的好感度
        int fav1 = PlayerPrefs.GetInt("LoadFav1", 0);
        int fav2 = PlayerPrefs.GetInt("LoadFav2", 0);
        int fav3 = PlayerPrefs.GetInt("LoadFav3", 0);
        GameManager.Instance.SetFavorability(1, fav1);
        GameManager.Instance.SetFavorability(2, fav2);
        GameManager.Instance.SetFavorability(3, fav3);

        // 清除临时键，避免下次进入剧情时被误用
        PlayerPrefs.DeleteKey("LoadDialogueId");
        PlayerPrefs.DeleteKey("LoadFav1");
        PlayerPrefs.DeleteKey("LoadFav2");
        PlayerPrefs.DeleteKey("LoadFav3");
        PlayerPrefs.Save();

        StartDialogue(loadDialogueId);
        yield break;
    }

    // 优先级2：从旧的单存档文件读取（兼容旧版本）
    SaveData data = null;
    if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.GetCurrentUser()))
        data = SaveManager.Instance.LoadGame(SaveManager.Instance.GetCurrentUser());

    if (data != null && data.currentDialogueId > 0 && data.selectedCharacter == selectedCharacter)
    {
        GameManager.Instance.SetFavorability(1, data.favorability1);
        GameManager.Instance.SetFavorability(2, data.favorability2);
        GameManager.Instance.SetFavorability(3, data.favorability3);
        StartDialogue(data.currentDialogueId);
    }
    else
    {
        // 没有存档或角色不匹配，从头开始
        switch (selectedCharacter)
        {
            case 1: StartDialogue(1001); break;
            case 2: StartDialogue(2001); break;
            case 3: StartDialogue(3001); break;
            default: StartDialogue(1001); break;
        }
    }
}


    // 开始显示指定id的对话
    public void StartDialogue(int dialogueId)
    {
        Dialogue dialogue = GameManager.Instance.GetDialogue(dialogueId);

        if (dialogue == null)
        {
            Debug.LogWarning("找不到对话ID: " + dialogueId);
            EndDialogue();
            return;
        }

        currentDialogue = dialogue;
        ShowDialogue(dialogue);
    }

    // 显示对话内容
    void ShowDialogue(Dialogue dialogue)
{
    dialoguePanel.SetActive(true);
    characterNameText.text = dialogue.characterName;
    dialogueText.text = dialogue.text;

    // 显示立绘
    CharacterDisplay charDisplay = FindObjectOfType<CharacterDisplay>();
    if (charDisplay != null)
        charDisplay.ShowCharacter(dialogue.characterId, dialogue.expression);

    // 加载背景图
    if (backgroundImage != null && !string.IsNullOrEmpty(dialogue.backgroundPath))
    {
        Sprite bg = Resources.Load<Sprite>(dialogue.backgroundPath);
        if (bg != null)
            backgroundImage.sprite = bg;
    }

    ClearChoiceButtons();

    // 如果是结局节点（只有一个选项且指向 -1），不显示"完成"按钮，自动结束
    bool isEnding = dialogue.choices != null
                    && dialogue.choices.Count == 1
                    && dialogue.choices[0].nextDialogueId == -1;

    if (isEnding)
    {
        choicePanel.SetActive(false);
        // 显示结局文字 3 秒后自动结束
        StartCoroutine(AutoEndAfter(3f));
    }
    else if (dialogue.choices != null && dialogue.choices.Count > 0)
    {
        choicePanel.SetActive(true);
        foreach (var choice in dialogue.choices)
            CreateChoiceButton(choice);
    }
    else
    {
        choicePanel.SetActive(false);
    }
}

System.Collections.IEnumerator AutoEndAfter(float delay)
{
    yield return new WaitForSeconds(delay);
    EndDialogue();
}
    

    // 动态生成选项按钮
    void CreateChoiceButton(Choice choice)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
        currentChoiceButtons.Add(btnObj);

        // 设置按钮文字
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
            btnText.text = choice.text;

        // 绑定点击事件
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            // 必须用局部变量捕获，否则闭包会出错
            Choice capturedChoice = choice;
            btn.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }

    // 玩家选择选项时触发
   
    void OnChoiceSelected(Choice choice)
    {
        if (choice.favorabilityChanges != null)
        {
            foreach (var change in choice.favorabilityChanges)
            {
                GameManager.Instance.UpdateFavorability(change.characterId, change.change);

                // 成就触发：好感度变化后检查
                if (AchievementManager.Instance != null)
                {
                    int newVal = GameManager.Instance.GetFavorability(change.characterId);
                    AchievementManager.Instance.OnFavorabilityChanged(change.characterId, newVal);
                }
            }
        }

        // 刷新好感度UI
        FavorabilityUI favUI = FindObjectOfType<FavorabilityUI>();
        if (favUI != null)
            favUI.UpdateHearts();

        // 结局分流：根据好感度决定走完美结局还是悲伤结局
        int nextId = RouteEnding(choice.nextDialogueId);

        // 成就触发：进入结局节点
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnEndingReached(nextId);

        if (nextId == -1)
            EndDialogue();
        else
            StartDialogue(nextId);
    }

    // 当 nextId 是已知的结局节点时，根据当前角色好感度决定走 GE 还是 BE
    // GE阈值=80：好感度≥80 走完美结局，否则走悲伤结局
    const int ENDING_THRESHOLD = 120;

    int RouteEnding(int nextId)
    {
        // 林晨西：完美1051 / 悲伤1052
        if (nextId == 1051)
            return GameManager.Instance.GetFavorability(1) >= ENDING_THRESHOLD ? 1051 : 1052;

        // 顾云深：完美2056 / 悲伤2057
        if (nextId == 2056)
            return GameManager.Instance.GetFavorability(2) >= ENDING_THRESHOLD ? 2056 : 2057;

        // 夏星河：完美3053 / 悲伤3054
        if (nextId == 3053)
            return GameManager.Instance.GetFavorability(3) >= ENDING_THRESHOLD ? 3053 : 3054;

        return nextId;
    }
   
    // 让 PauseManager 能读取当前对话 ID
    public int GetCurrentDialogueId()
    {
        return currentDialogue != null ? currentDialogue.id : 0;
    }

    // 清除所有选项按钮
    void ClearChoiceButtons()
    {
        foreach (var btn in currentChoiceButtons)
        {
            Destroy(btn);
        }
        currentChoiceButtons.Clear();
    }

    // 对话结束
    void EndDialogue()
    {
        Debug.Log("对话结束");
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        ClearChoiceButtons();

        // 剧情结束后延迟2秒自动回到 HomePage
        StartCoroutine(BackToHomePageAfterDelay(2f));
    }

    System.Collections.IEnumerator BackToHomePageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("HomePage");
    }
}