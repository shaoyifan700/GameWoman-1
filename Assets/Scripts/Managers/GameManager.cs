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
        if (Instance == null)
        {
            Instance = this;
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
            string json = File.ReadAllText(characterPath);
            characterData = JsonUtility.FromJson<CharacterDataWrapper>(json);
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
        currentFavorability = new Dictionary<int, int>();
        if (characterData != null)
        {
            foreach (var character in characterData.characters)
            {
                currentFavorability[character.id] = 0;
            }
        }
    }

    public Dialogue GetDialogue(int dialogueId)
    {
        if (dialogueData != null)
        {
            return dialogueData.dialogues.Find(d => d.id == dialogueId);
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

    public int GetFavorability(int characterId)
    {
        return currentFavorability.ContainsKey(characterId) ? currentFavorability[characterId] : 0;
    }
}
