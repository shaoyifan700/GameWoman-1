using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AIChatSceneManager : MonoBehaviour
{
    [Header("UI引用")]
    public Transform characterGrid;
    public GameObject characterCardPrefab;
    public TextMeshProUGUI staminaText;
    public RectTransform contentRect;

    [Header("卡片设置")]
    public float cardHeight = 80f;

    void Start()
    {
        Debug.Log("AIChatSceneManager 执行了！");
        RefreshStamina();
        StartCoroutine(LoadAfterLayout());
        
    }
    void RefreshStamina()
{
    if (staminaText != null && StaminaManager.Instance != null)
        staminaText.text = "体力：" + StaminaManager.Instance.GetStamina();
}

    IEnumerator LoadAfterLayout()
    {
        yield return null;
        yield return null;

        float cardWidth = contentRect.rect.width;
        if (cardWidth <= 0) cardWidth = Screen.width;
        Debug.Log("cardWidth: " + cardWidth);

        LoadCharacters(cardWidth);

        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Debug.Log("Content Height after rebuild: " + contentRect.rect.height);
    }

    void LoadCharacters(float cardWidth)
    {
        Debug.Log("LoadCharacters 开始");
        string path = Application.streamingAssetsPath + "/Data/chat_characters.json";
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        ChatCharacterWrapper wrapper = JsonUtility.FromJson<ChatCharacterWrapper>(json);
        Debug.Log("角色数量：" + wrapper.characters.Count);

        foreach (var character in wrapper.characters)
        {
            Debug.Log("生成角色：" + character.name);
            GameObject card = Instantiate(characterCardPrefab, characterGrid);

            // 加Layout Element告诉Layout Group这个卡片的高度
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = cardHeight;
            le.preferredWidth = cardWidth;
            

            TextMeshProUGUI nameTMP = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI tagTMP = card.transform.Find("TagText")?.GetComponent<TextMeshProUGUI>();

            if (nameTMP != null) nameTMP.text = character.name;
            if (tagTMP != null) tagTMP.text = character.tag;

            // 加载头像
            Image avatarImage = card.transform.Find("Image")?.GetComponent<Image>();
            if (avatarImage != null && !string.IsNullOrEmpty(character.avatar))
            {
                Sprite sprite = Resources.Load<Sprite>(character.avatar);
                if (sprite != null)
                {
                    avatarImage.sprite = sprite;
                    avatarImage.color = Color.white;  // 重置颜色，避免绿色染色
                }
                else
                {
                    Debug.LogWarning("找不到头像: " + character.avatar);
                }
            }

            Button btn = card.GetComponent<Button>();
            int capturedId = character.id;
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("AIChatCharacter", capturedId);
                    SceneManager.LoadScene("AIChatDialogue");
                });
        }
    }

    public void OnClickRecharge()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("HomePage");
    }
}