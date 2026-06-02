using UnityEngine;
using System.Collections.Generic;
using System.IO;




public class GameManager : MonoBehaviour
{
    public static GameManager Instance;//单例manager

    private CharacterDataWrapper characterData;// 存储3个角色的信息
    private DialogueDataWrapper dialogueData;// 存储所有对话
    private Dictionary<int, int> currentFavorability;//键值对存储好感度
    

    void Awake()//确保单例的manager
    {
        if (Instance == null)// 问：内存里现在有没有这个Manager？
        {
            Instance = this; // 没有 → 我来当唯一实例
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()//初始化数据
    {
        LoadGameData();
        InitializeFavorability();
    }

    void LoadGameData()
    {
        // 加载角色数据
        string characterPath = Application.streamingAssetsPath + "/Data/characters.json";
        if (File.Exists(characterPath))
        {
            string json = File.ReadAllText(characterPath);// 吧文件内容读成字符串
            characterData = JsonUtility.FromJson<CharacterDataWrapper>(json);// 把字符串解析成CharacterDataWrapper对象
            Debug.Log("角色数据加载成功,共" + characterData.characters.Count + "个角色");
        }
        else
        {
            Debug.LogError("找不到角色数据文件: " + characterPath);
        }

        // 加载对话数据
        string dialoguePath = Application.streamingAssetsPath + "/Data/dialogues.json";
        if (File.Exists(dialoguePath))
        {
            string json = File.ReadAllText(dialoguePath);
            dialogueData = JsonUtility.FromJson<DialogueDataWrapper>(json);
            Debug.Log("对话数据加载成功,共" + dialogueData.dialogues.Count + "条对话");
        }
        else
        {
            Debug.LogError("找不到对话数据文件: " + dialoguePath);
        }
    }

    void InitializeFavorability()//初始化好感度
    {
        currentFavorability = new Dictionary<int, int>();// 创建一个空的字典，Key=角色id，Value=好感度数值

        if (characterData != null)
        {
            foreach (var character in characterData.characters)
            {
                currentFavorability[character.id] = 0;// 每个角色的初始好感度设为 0
            }
        }
    }

    public Dialogue GetDialogue(int dialogueId)
    {
        if (dialogueData != null)
        {
            return dialogueData.dialogues.Find(d => d.id == dialogueId);// 在对话列表里找第一个 id 匹配的对话并返回
        }
        return null;
    }

    public void UpdateFavorability(int characterId, int change)
    {
        if (currentFavorability.ContainsKey(characterId))
        {
            currentFavorability[characterId] += change;
            Debug.Log("角色" + characterId + "好感度变化: " + (change > 0 ? "+" : "") + change +
                     " (当前: " + currentFavorability[characterId] + ")");
        }
    }

    // 直接设置好感度（用于读档恢复，避免累加导致数值翻倍）
    public void SetFavorability(int characterId, int value)
    {
        if (currentFavorability == null) currentFavorability = new Dictionary<int, int>();
        currentFavorability[characterId] = value;
    }

    // 重置所有好感度（用于读档前清零）, 读档前清零，是为了防止当前游戏里的旧数据"混进"新读取的存档里。
    public void ResetAllFavorability()
    {
        if (currentFavorability == null) return;
        var keys = new List<int>(currentFavorability.Keys);
        foreach (var k in keys) currentFavorability[k] = 0;
    }

    public int GetFavorability(int characterId)// 先检查字典里有没有这个角色id
    // 有 → 返回对应的好感度数值
    // 没有 → 返回 0，避免报错
    {
        return currentFavorability.ContainsKey(characterId) ? currentFavorability[characterId] : 0;
    }
}
