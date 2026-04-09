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

    switch (selectedCharacter)
    {
        case 1:
            StartDialogue(1001); // 林晨西
            break;
        case 2:
            StartDialogue(2001); // 顾云深
            break;
        case 3:
            StartDialogue(3001); // 夏星河
            break;
        default:
            StartDialogue(1001);
            break;
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

        
        // 显示角色名和对话文本
        dialoguePanel.SetActive(true);
        characterNameText.text = dialogue.characterName;
        dialogueText.text = dialogue.text;

        // 清除旧的选项按钮
        ClearChoiceButtons();

        // 生成新的选项按钮
        if (dialogue.choices != null && dialogue.choices.Count > 0)
        {
            choicePanel.SetActive(true);
            foreach (var choice in dialogue.choices)
            {
                CreateChoiceButton(choice);
            }
        }
        else
        {
            choicePanel.SetActive(false);
        }
        
        // 加载背景图
        if (backgroundImage != null && !string.IsNullOrEmpty(dialogue.backgroundPath))
        {
            Sprite bg = Resources.Load<Sprite>(dialogue.backgroundPath);
            if (bg != null)
                backgroundImage.sprite = bg;
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

        if (choice.nextDialogueId == -1)
            EndDialogue();
        else
            StartDialogue(choice.nextDialogueId);
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