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
        string users = PlayerPrefs.GetString("UserList", "");
        var userList = new System.Collections.Generic.List<string>(users.Split(','));
        userList.Remove(userName);
        PlayerPrefs.SetString("UserList", string.Join(",", userList));
        PlayerPrefs.Save();
    }
}