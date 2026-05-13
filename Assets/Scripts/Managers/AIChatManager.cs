using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

[System.Serializable]
public class DoubaoMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class DoubaoRequest
{
    public string model = "doubao-seed-character-251128";
    public int max_tokens = 300;
    public List<DoubaoMessage> messages = new List<DoubaoMessage>();
}

public class AIChatManager : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterTagText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI typingText;
    public Transform chatContent;
    public GameObject playerBubblePrefab;
    public GameObject characterBubblePrefab;
    public TMP_InputField inputField;
    public Button btnSend;
    public ScrollRect scrollRect;

    [Header("API设置")]
    public string apiKey = "ark-cf33ce83-a990-4c78-a0bc-0541cd23297b-f0e16";

    [Header("体力不足弹窗")]
    public GameObject insufficientStaminaPanel;

    private ChatCharacter currentCharacter;
    private List<DoubaoMessage> messageHistory = new List<DoubaoMessage>();
    private List<ChatMessageRecord> savedRecords = new List<ChatMessageRecord>();
    private bool isWaiting = false;

    void Start()
    {
        int characterId = PlayerPrefs.GetInt("AIChatCharacter", 1);
        LoadCharacter(characterId);

        typingText.gameObject.SetActive(false);
        btnSend.onClick.AddListener(OnClickSend);

        if (StaminaManager.Instance != null && SaveManager.Instance != null)
            StaminaManager.Instance.InitStamina(SaveManager.Instance.GetCurrentUser());

        RefreshStamina();
        StartCoroutine(LoadChatHistoryDelayed());
    }

    IEnumerator LoadChatHistoryDelayed()
    {
        yield return null;
        LoadChatHistory();
    }

    void LoadCharacter(int characterId)
    {
        string path = Application.streamingAssetsPath + "/Data/chat_characters.json";
        if (!File.Exists(path)) { Debug.LogError("找不到chat_characters.json"); return; }

        string json = File.ReadAllText(path);
        ChatCharacterWrapper wrapper = JsonUtility.FromJson<ChatCharacterWrapper>(json);
        foreach (var c in wrapper.characters)
        {
            if (c.id == characterId) { currentCharacter = c; break; }
        }

        if (currentCharacter != null)
        {
            if (characterNameText != null) characterNameText.text = currentCharacter.name;
            if (characterTagText  != null) characterTagText.text  = currentCharacter.tag;
            messageHistory.Add(new DoubaoMessage { role = "system", content = currentCharacter.systemPrompt });
        }
    }

    string GetHistoryKey()
    {
        string user = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "default";
        int charId = currentCharacter != null ? currentCharacter.id : 0;
        return "ChatHistory_" + user + "_" + charId;
    }

    void LoadChatHistory()
    {
        string key = GetHistoryKey();
        if (!PlayerPrefs.HasKey(key)) return;

        ChatHistoryWrapper wrapper = JsonUtility.FromJson<ChatHistoryWrapper>(PlayerPrefs.GetString(key));
        if (wrapper == null || wrapper.messages == null) return;

        savedRecords = wrapper.messages;

        for (int i = 0; i < savedRecords.Count; i++)
        {
            var record = savedRecords[i];
            messageHistory.Add(new DoubaoMessage { role = record.role, content = record.content });

            if (record.role == "user")
                SpawnBubble(playerBubblePrefab, record.content, -1, 0);
            else if (record.role == "assistant")
                SpawnBubble(characterBubblePrefab, record.content, i, record.reaction);
        }

        ScrollToBottom();
    }

    void SaveChatHistory()
    {
        ChatHistoryWrapper wrapper = new ChatHistoryWrapper { messages = new List<ChatMessageRecord>(savedRecords) };
        PlayerPrefs.SetString(GetHistoryKey(), JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    void RefreshStamina()
    {
        if (staminaText != null && StaminaManager.Instance != null)
            staminaText.text = "体力：" + StaminaManager.Instance.GetStamina();
    }

    public void OnClickSend()
    {
        if (isWaiting) return;
        string playerText = inputField.text.Trim();
        if (string.IsNullOrEmpty(playerText)) return;

        if (StaminaManager.Instance != null && !StaminaManager.Instance.UseStamina(3))
        {
            ShowInsufficientStaminaPopup();
            return;
        }

        RefreshStamina();
        inputField.text = "";
        btnSend.interactable = false;

        SpawnBubble(playerBubblePrefab, playerText, -1, 0);
        messageHistory.Add(new DoubaoMessage { role = "user", content = playerText });
        savedRecords.Add(new ChatMessageRecord { role = "user", content = playerText, reaction = 0 });

        StartCoroutine(CallDoubaoAPI());
    }

    IEnumerator CallDoubaoAPI()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.3f);
        typingText.gameObject.SetActive(true);

        DoubaoRequest request = new DoubaoRequest { messages = new List<DoubaoMessage>(messageHistory) };
        string jsonBody = JsonUtility.ToJson(request);

        UnityWebRequest www = new UnityWebRequest("https://ark.cn-beijing.volces.com/api/v3/chat/completions", "POST");
        www.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return www.SendWebRequest();

        typingText.gameObject.SetActive(false);

        if (www.result == UnityWebRequest.Result.Success)
        {
            string replyText = ParseResponse(www.downloadHandler.text);
            messageHistory.Add(new DoubaoMessage { role = "assistant", content = replyText });
            savedRecords.Add(new ChatMessageRecord { role = "assistant", content = replyText, reaction = 0 });
            SaveChatHistory();
            int idx = savedRecords.Count - 1;
            StartCoroutine(ShowTypingEffect(replyText, idx));
        }
        else
        {
            Debug.LogError("API失败：" + www.error);
            SpawnBubble(characterBubblePrefab, "（网络出了点问题，稍后再试试吧）", -1, 0);
            btnSend.interactable = true;
        }

        isWaiting = false;
    }

    // 统一气泡创建入口
    // recordIndex >= 0 时为角色气泡，自动附加赞/踩行
    GameObject SpawnBubble(GameObject prefab, string text, int recordIndex, int savedReaction)
    {
        GameObject bubble = Instantiate(prefab, chatContent);
        var tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;

        if (recordIndex >= 0)
            SetupReactionButtons(bubble, recordIndex, savedReaction);

        ScrollToBottom();
        return bubble;
    }

    // 打字机效果，结束后附赞/踩行
    IEnumerator ShowTypingEffect(string text, int recordIndex)
    {
        GameObject bubble = Instantiate(characterBubblePrefab, chatContent);
        var tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = "";

        foreach (char c in text)
        {
            tmp.text += c;
            ScrollToBottom();
            yield return new WaitForSeconds(0.03f);
        }

        SetupReactionButtons(bubble, recordIndex, 0);
        ScrollToBottom();
        btnSend.interactable = true;
    }

    // 从预制体里找 BtnLike / BtnDislike，绑定点击和颜色
    void SetupReactionButtons(GameObject bubble, int recordIndex, int savedReaction)
    {
        Button likeBtn = null, dislikeBtn = null;
        foreach (var t in bubble.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Trim() == "BtnLike")    likeBtn    = t.GetComponent<Button>();
            if (t.name.Trim() == "BtnDislike") dislikeBtn = t.GetComponent<Button>();
        }
        if (likeBtn == null || dislikeBtn == null)
        {
            Debug.LogWarning("找不到 BtnLike 或 BtnDislike，请检查预制体子物体名称");
            return;
        }

        var likeTMP    = likeBtn.GetComponentInChildren<TextMeshProUGUI>();
        var dislikeTMP = dislikeBtn.GetComponentInChildren<TextMeshProUGUI>();

        bool liked    = savedReaction == 1;
        bool disliked = savedReaction == -1;

        UpdateReactionText(likeTMP,    liked,    true);
        UpdateReactionText(dislikeTMP, disliked, false);

        likeBtn.onClick.RemoveAllListeners();
        dislikeBtn.onClick.RemoveAllListeners();

        likeBtn.onClick.AddListener(() =>
        {
            liked = !liked;
            if (liked) disliked = false;
            UpdateReactionText(likeTMP,    liked,    true);
            UpdateReactionText(dislikeTMP, disliked, false);
            if (recordIndex >= 0 && recordIndex < savedRecords.Count)
            {
                savedRecords[recordIndex].reaction = liked ? 1 : 0;
                SaveChatHistory();
            }
        });

        dislikeBtn.onClick.AddListener(() =>
        {
            disliked = !disliked;
            if (disliked) liked = false;
            UpdateReactionText(likeTMP,    liked,    true);
            UpdateReactionText(dislikeTMP, disliked, false);
            if (recordIndex >= 0 && recordIndex < savedRecords.Count)
            {
                savedRecords[recordIndex].reaction = disliked ? -1 : 0;
                SaveChatHistory();
            }
        });
    }

    void UpdateReactionText(TextMeshProUGUI tmp, bool active, bool isLike)
    {
        if (tmp == null) return;
        if (isLike)
        {
            tmp.text  = active ? "♥" : "▲ ";
            tmp.color = active ? new Color(1f, 0.2f, 0.2f) : new Color(0.4f, 0.4f, 0.4f);
            tmp.fontSize = active ? 22 : 16;
        }
        else
        {
            tmp.text  = active ? "<s>♥</s>" : "▼ ";
            tmp.color = active ? new Color(0.6f, 0.2f, 0.8f) : new Color(0.4f, 0.4f, 0.4f);
            tmp.fontSize = active ? 22 : 16;
        }
    }

    string ParseResponse(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            if (start < 11) return "……";
            int end = json.IndexOf("\"", start);
            if (end < 0) return "……";
            string text = json.Substring(start, end - start)
                .Replace("\\n", "\n").Replace("\\\"", "\"");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"（[^）]*）", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\([^\)]*\)", "");
            return text.Trim();
        }
        catch { return "……"; }
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ShowInsufficientStaminaPopup()
    {
        if (insufficientStaminaPanel != null) insufficientStaminaPanel.SetActive(true);
    }

    public void OnClickGoShop()   => SceneManager.LoadScene("ShopScene");
    public void OnClickClosePopup()
    {
        if (insufficientStaminaPanel != null) insufficientStaminaPanel.SetActive(false);
    }
    public void OnClickBack() => SceneManager.LoadScene("AIChatScene");
}
