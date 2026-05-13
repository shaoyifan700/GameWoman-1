using UnityEngine;
using System;

[Serializable]
public class SaveData
{
    public string userName;
    public int currentDialogueId;
    public int selectedCharacter;
    public int favorability1; // 林晨西
    public int favorability2; // 顾云深
    public int favorability3; // 夏星河
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string currentUser = "";

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
        return;
    }
}

    // 设置当前用户
    public void SetCurrentUser(string userName)
    {
        currentUser = userName;
    }

    public string GetCurrentUser()
    {
        return currentUser;
    }

    // 保存存档
    public void SaveGame(int currentDialogueId)
{
    Debug.Log("当前用户：" + currentUser); // 加这行
    if (string.IsNullOrEmpty(currentUser)) return;

    SaveData data = new SaveData();
    data.userName = currentUser;
    data.currentDialogueId = currentDialogueId;
    data.selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1);
    data.favorability1 = GameManager.Instance.GetFavorability(1);
    data.favorability2 = GameManager.Instance.GetFavorability(2);
    data.favorability3 = GameManager.Instance.GetFavorability(3);

    string json = JsonUtility.ToJson(data);
    PlayerPrefs.SetString("Save_" + currentUser, json);
    PlayerPrefs.Save();

    // 同时保存到 PlayerPrefs 方便读取
    PlayerPrefs.SetInt("SavedCharacter", data.selectedCharacter);
    PlayerPrefs.SetInt("LoadDialogueId", currentDialogueId);
    PlayerPrefs.SetInt("LoadFav1", data.favorability1);
    PlayerPrefs.SetInt("LoadFav2", data.favorability2);
    PlayerPrefs.SetInt("LoadFav3", data.favorability3);
    PlayerPrefs.Save();

    Debug.Log("存档成功：" + currentUser);
}

    // 读取存档
    public SaveData LoadGame(string userName)
    {
        string key = "Save_" + userName;
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("读取存档成功：" + userName);
            return data;
        }
        Debug.Log("没有找到存档：" + userName);
        return null;
    }

    // 检查是否有存档
    public bool HasSave(string userName)
    {
        return PlayerPrefs.HasKey("Save_" + userName);
    }

    // 删除存档
    public void DeleteSave(string userName)
    {
        PlayerPrefs.DeleteKey("Save_" + userName);
        PlayerPrefs.Save();
        Debug.Log("删除存档：" + userName);
    }

    // ===== 5槽位存档系统 =====

    public const int MAX_SLOTS = 5;

    string SlotKey(string userName, int slotIndex) => "SaveSlot_" + userName + "_" + slotIndex;

    // 保存到指定槽位
    public void SaveToSlot(int slotIndex, int currentDialogueId, string sceneName)
    {
        if (string.IsNullOrEmpty(currentUser)) return;
        if (slotIndex < 1 || slotIndex > MAX_SLOTS) return;

        SaveSlot slot = new SaveSlot();
        slot.slotIndex = slotIndex;
        slot.timestamp = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        slot.sceneName = sceneName;
        slot.currentDialogueId = currentDialogueId;
        slot.selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 1);
        slot.favorability1 = GameManager.Instance != null ? GameManager.Instance.GetFavorability(1) : 0;
        slot.favorability2 = GameManager.Instance != null ? GameManager.Instance.GetFavorability(2) : 0;
        slot.favorability3 = GameManager.Instance != null ? GameManager.Instance.GetFavorability(3) : 0;

        // 角色名
        switch (slot.selectedCharacter)
        {
            case 1: slot.characterName = "林晨西"; break;
            case 2: slot.characterName = "顾云深"; break;
            case 3: slot.characterName = "夏星河"; break;
            default: slot.characterName = "未知"; break;
        }
        slot.saveName = slot.characterName + "线";
        slot.isEmpty = false;

        string json = JsonUtility.ToJson(slot);
        PlayerPrefs.SetString(SlotKey(currentUser, slotIndex), json);
        PlayerPrefs.Save();
        Debug.Log("存档至槽位 " + slotIndex + " 成功");
    }

    // 读取指定槽位
    public SaveSlot LoadFromSlot(int slotIndex)
    {
        if (string.IsNullOrEmpty(currentUser)) return null;
        if (slotIndex < 1 || slotIndex > MAX_SLOTS) return null;

        string key = SlotKey(currentUser, slotIndex);
        if (!PlayerPrefs.HasKey(key)) return null;

        return JsonUtility.FromJson<SaveSlot>(PlayerPrefs.GetString(key));
    }

    // 删除指定槽位
    public void DeleteSlot(int slotIndex)
    {
        if (string.IsNullOrEmpty(currentUser)) return;
        if (slotIndex < 1 || slotIndex > MAX_SLOTS) return;
        PlayerPrefs.DeleteKey(SlotKey(currentUser, slotIndex));
        PlayerPrefs.Save();
    }

    // 获取5个槽位的列表（空槽返回 isEmpty=true 的 SaveSlot）
    public SaveSlot[] GetAllSlots()
    {
        SaveSlot[] slots = new SaveSlot[MAX_SLOTS];
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            int idx = i + 1;
            SaveSlot loaded = LoadFromSlot(idx);
            if (loaded != null) slots[i] = loaded;
            else slots[i] = new SaveSlot { slotIndex = idx, isEmpty = true };
        }
        return slots;
    }

    // ===== 密码相关 =====

    // 设置密码（创建用户时调用）
    public void SetPassword(string userName, string password)
    {
        PlayerPrefs.SetString("Password_" + userName, password);
        PlayerPrefs.Save();
    }

    // 验证密码
    public bool VerifyPassword(string userName, string password)
    {
        string saved = PlayerPrefs.GetString("Password_" + userName, "");
        return saved == password;
    }

    // 是否设置过密码
    public bool HasPassword(string userName)
    {
        return PlayerPrefs.HasKey("Password_" + userName);
    }

    // 保存用户列表
    public void AddUserToList(string userName)
    {
        string users = PlayerPrefs.GetString("UserList", "");
        if (!users.Contains(userName))
        {
            users = string.IsNullOrEmpty(users) ? userName : users + "," + userName;
            PlayerPrefs.SetString("UserList", users);
            PlayerPrefs.Save();
        }
    }

    // 获取所有用户
    public string[] GetAllUsers()
    {
        string users = PlayerPrefs.GetString("UserList", "");
        if (string.IsNullOrEmpty(users)) return new string[0];
        return users.Split(',');
    }

    // 删除用户
    public void DeleteUser(string userName)
    {
        DeleteSave(userName);

        // 清除体力、钱包、密码数据
        PlayerPrefs.DeleteKey("Stamina_" + userName);
        PlayerPrefs.DeleteKey("Wallet_" + userName);
        PlayerPrefs.DeleteKey("Password_" + userName);

        // 清除所有角色的聊天记录
        for (int i = 1; i <= 20; i++)
            PlayerPrefs.DeleteKey("ChatHistory_" + userName + "_" + i);

        // 清除所有存档槽
        for (int i = 1; i <= MAX_SLOTS; i++)
            PlayerPrefs.DeleteKey("SaveSlot_" + userName + "_" + i);

        string users = PlayerPrefs.GetString("UserList", "");
        var userList = new System.Collections.Generic.List<string>(users.Split(','));
        userList.Remove(userName);
        PlayerPrefs.SetString("UserList", string.Join(",", userList));
        PlayerPrefs.Save();
    }
}