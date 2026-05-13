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

    // 直接从存档文件读取，不依赖 PlayerPrefs 的 LoadDialogueId
    SaveData data = null;
    if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.GetCurrentUser()))
        data = SaveManager.Instance.LoadGame(SaveManager.Instance.GetCurrentUser());

    if (data != null && data.currentDialogueId > 0 && data.selectedCharacter == selectedCharacter)
    {
        // 有存档且角色匹配，从存档位置继续
        GameManager.Instance.UpdateFavorability(1, data.favorability1);
        GameManager.Instance.UpdateFavorability(2, data.favorability2);
        GameManager.Instance.UpdateFavorability(3, data.favorability3);
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

    if (dialogue.choices != null && dialogue.choices.Count > 0)
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
            }
        }

        // 刷新好感度UI
        FavorabilityUI favUI = FindObjectOfType<FavorabilityUI>();
        if (favUI != null)
            favUI.UpdateHearts();

        // 结局分流：根据好感度决定走完美结局还是悲伤结局
        int nextId = RouteEnding(choice.nextDialogueId);

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
    }
}