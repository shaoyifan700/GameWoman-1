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

    private ChatCharacter currentCharacter;
    private List<DoubaoMessage> messageHistory = new List<DoubaoMessage>();
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
    }

    void LoadCharacter(int characterId)
    {
        string path = Application.streamingAssetsPath + "/Data/chat_characters.json";
        if (!File.Exists(path))
        {
            Debug.LogError("找不到chat_characters.json");
            return;
        }

        string json = File.ReadAllText(path);
        ChatCharacterWrapper wrapper = JsonUtility.FromJson<ChatCharacterWrapper>(json);

        foreach (var c in wrapper.characters)
        {
            if (c.id == characterId)
            {
                currentCharacter = c;
                break;
            }
        }

        if (characterNameText != null && currentCharacter != null)
            characterNameText.text = currentCharacter.name;
        if (characterTagText != null && currentCharacter != null)
            characterTagText.text = currentCharacter.tag;

        // 加入系统提示作为第一条消息
        if (currentCharacter != null)
        {
            messageHistory.Add(new DoubaoMessage
            {
                role = "system",
                content = currentCharacter.systemPrompt
            });
        }
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

        if (StaminaManager.Instance != null && !StaminaManager.Instance.UseStamina(2))
        {
            SceneManager.LoadScene("ShopScene");
            return;
        }

        RefreshStamina();
        inputField.text = "";
        btnSend.interactable = false;

        ShowBubble(playerBubblePrefab, playerText);
        messageHistory.Add(new DoubaoMessage { role = "user", content = playerText });

        StartCoroutine(CallDoubaoAPI());
    }

    IEnumerator CallDoubaoAPI()
    {
        isWaiting = true;

        yield return new WaitForSeconds(0.3f);
        typingText.gameObject.SetActive(true);

        DoubaoRequest request = new DoubaoRequest();
        request.messages = new List<DoubaoMessage>(messageHistory);

        string jsonBody = JsonUtility.ToJson(request);

        UnityWebRequest www = new UnityWebRequest(
            "https://ark.cn-beijing.volces.com/api/v3/chat/completions", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return www.SendWebRequest();

        typingText.gameObject.SetActive(false);

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseJson = www.downloadHandler.text;
            Debug.Log("API回复：" + responseJson);
            string replyText = ParseResponse(responseJson);

            messageHistory.Add(new DoubaoMessage { role = "assistant", content = replyText });
            StartCoroutine(ShowTypingEffect(replyText));
        }
        else
        {
            Debug.LogError("API失败：" + www.error + "\n" + www.downloadHandler.text);
            ShowBubble(characterBubblePrefab, "（网络出了点问题，稍后再试试吧）");
            btnSend.interactable = true;
        }

        isWaiting = false;
    }

    string ParseResponse(string json)
    {
        try
        {
             int contentIndex = json.IndexOf("\"content\":\"") + 11;
        if (contentIndex < 11) return "……";
        int endIndex = json.IndexOf("\"", contentIndex);
        if (endIndex < 0) return "……";
        string text = json.Substring(contentIndex, endIndex - contentIndex);
        text = text.Replace("\\n", "\n").Replace("\\\"", "\"");

        // 去掉中文括号和英文括号里的内容
        text = System.Text.RegularExpressions.Regex.Replace(text, @"（[^）]*）", "");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\([^\)]*\)", "");
        text = text.Trim();

        return text;
        }
        catch
        {
            return "……";
        }
    }

    IEnumerator ShowTypingEffect(string text)
    {
        GameObject bubble = Instantiate(characterBubblePrefab, chatContent);
        TextMeshProUGUI tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = "";

        foreach (char c in text)
        {
            tmp.text += c;
            ScrollToBottom();
            yield return new WaitForSeconds(0.03f);
        }

        ScrollToBottom();
        btnSend.interactable = true;
    }

    void ShowBubble(GameObject prefab, string text)
    {
        GameObject bubble = Instantiate(prefab, chatContent);
        TextMeshProUGUI tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = text;
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("AIChatScene");
    }
}