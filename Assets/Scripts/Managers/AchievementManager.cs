using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// 成就管理器：单例，负责加载成就定义、记录玩家进度、触发解锁、通知UI。
/// 数据按用户隔离存储（PlayerPrefs key = AchievementProgress_{用户名}）。
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    // 成就定义（从JSON加载，所有用户共享）
    private List<AchievementDef> definitions = new List<AchievementDef>();

    // 当前用户的解锁进度
    private Dictionary<string, AchievementProgress> progress = new Dictionary<string, AchievementProgress>();

    // 已完成结局的角色集合（用于"多面人身"判定）
    private HashSet<int> charactersWithEnding = new HashSet<int>();

    // 解锁通知（UI层通过这个回调显示弹窗）
    public event Action<AchievementDef> OnAchievementUnlocked;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDefinitions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadDefinitions()
    {
        string path = Application.streamingAssetsPath + "/Data/achievements.json";
        if (!File.Exists(path))
        {
            Debug.LogError("找不到成就配置文件: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        AchievementDefWrapper wrapper = JsonUtility.FromJson<AchievementDefWrapper>(json);
        if (wrapper != null && wrapper.achievements != null)
            definitions = wrapper.achievements;

        Debug.Log("成就定义加载完成，共 " + definitions.Count + " 个");
    }

    // ─────────────────────────────────────────────
    // 数据加载与保存
    // ─────────────────────────────────────────────

    string ProgressKey() {
        string user = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "";
        return "AchievementProgress_" + user;
    }

    string EndingsKey() {
        string user = SaveManager.Instance != null ? SaveManager.Instance.GetCurrentUser() : "";
        return "AchievementEndings_" + user;
    }

    public void LoadProgressForCurrentUser()
    {
        progress.Clear();
        charactersWithEnding.Clear();

        // 加载成就进度
        string key = ProgressKey();
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            AchievementProgressWrapper w = JsonUtility.FromJson<AchievementProgressWrapper>(json);
            if (w != null && w.items != null)
                foreach (var p in w.items) progress[p.id] = p;
        }

        // 加载已完成结局的角色列表
        string endingsKey = EndingsKey();
        if (PlayerPrefs.HasKey(endingsKey))
        {
            string s = PlayerPrefs.GetString(endingsKey);
            foreach (string part in s.Split(','))
                if (int.TryParse(part, out int id)) charactersWithEnding.Add(id);
        }
    }

    void SaveProgress()
    {
        AchievementProgressWrapper w = new AchievementProgressWrapper();
        foreach (var p in progress.Values) w.items.Add(p);
        PlayerPrefs.SetString(ProgressKey(), JsonUtility.ToJson(w));

        // 保存已完成结局角色
        List<string> parts = new List<string>();
        foreach (int id in charactersWithEnding) parts.Add(id.ToString());
        PlayerPrefs.SetString(EndingsKey(), string.Join(",", parts));

        PlayerPrefs.Save();
    }

    // ─────────────────────────────────────────────
    // 公共触发接口
    // ─────────────────────────────────────────────

    /// <summary>解锁指定成就。已解锁的不再重复触发弹窗。</summary>
    public void Unlock(string id)
    {
        if (progress.ContainsKey(id) && progress[id].unlocked) return;

        AchievementDef def = definitions.Find(d => d.id == id);
        if (def == null)
        {
            Debug.LogWarning("未知成就ID: " + id);
            return;
        }

        AchievementProgress p = new AchievementProgress
        {
            id = id,
            unlocked = true,
            unlockedTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
        };
        progress[id] = p;

        SaveProgress();
        Debug.Log("🏆 成就解锁: " + def.name);

        // 触发UI弹窗
        OnAchievementUnlocked?.Invoke(def);
    }

    /// <summary>好感度变化时调用。每24点1颗心：1心=24、2心=48、3心=72（HE阈值120=5颗满心）。</summary>
    public void OnFavorabilityChanged(int characterId, int newValue)
    {
        // 林晨西
        if (characterId == 1 && newValue >= 24)
            Unlock("lin_1hearts");
        if (characterId == 1 && newValue >= 48)
            Unlock("lin_2hearts");
        if (characterId == 1 && newValue >= 72)
            Unlock("lin_3hearts");

        // 顾云深
        if (characterId == 2 && newValue >= 24)
            Unlock("gu_1hearts");
        if (characterId == 2 && newValue >= 48)
            Unlock("gu_2hearts");
        if (characterId == 2 && newValue >= 72)
            Unlock("gu_3hearts");

        // 夏星河
        if (characterId == 3 && newValue >= 24)
            Unlock("xia_1hearts");
        if (characterId == 3 && newValue >= 48)
            Unlock("xia_2hearts");
        if (characterId == 3 && newValue >= 72)
            Unlock("xia_3hearts");
    }

    /// <summary>剧情进入结局节点时调用。</summary>
    public void OnEndingReached(int dialogueId)
    {
        switch (dialogueId)
        {
            case 1051: Unlock("lin_he"); MarkCharacterEndingDone(1); break;
            case 1052: Unlock("lin_be"); MarkCharacterEndingDone(1); break;
            case 2056: MarkCharacterEndingDone(2); break;
            case 2057: MarkCharacterEndingDone(2); break;
            case 3053: MarkCharacterEndingDone(3); break;
            case 3054: MarkCharacterEndingDone(3); break;
        }
    }

    void MarkCharacterEndingDone(int characterId)
    {
        charactersWithEnding.Add(characterId);
        SaveProgress();

        // 多面人身：三位角色都至少完成一条结局
        if (charactersWithEnding.Contains(1) &&
            charactersWithEnding.Contains(2) &&
            charactersWithEnding.Contains(3))
        {
            Unlock("duo_mian");
        }
    }

    // ─────────────────────────────────────────────
    // 查询接口（供UI层使用）
    // ─────────────────────────────────────────────

    public List<AchievementDef> GetAllDefinitions() { return definitions; }

    public bool IsUnlocked(string id)
    {
        return progress.ContainsKey(id) && progress[id].unlocked;
    }

    public string GetUnlockedTime(string id)
    {
        return progress.ContainsKey(id) ? progress[id].unlockedTime : "";
    }
}
