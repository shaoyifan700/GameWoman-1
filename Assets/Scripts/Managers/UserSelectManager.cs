using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UserSelectManager : MonoBehaviour
{
    public Transform userListPanel;      // 用户列表容器
    public GameObject userButtonPrefab;  // 用户按钮预制体
    public TMP_InputField inputField;    // 输入框
    public TextMeshProUGUI tipText;      // 提示文字

    void Start()
    {
        RefreshUserList();
    }

    // 刷新用户列表
   void RefreshUserList()
{
    foreach (Transform child in userListPanel)
        Destroy(child.gameObject);

    string[] users = SaveManager.Instance.GetAllUsers();
    foreach (string user in users)
    {
        if (string.IsNullOrEmpty(user)) continue;

        GameObject item = Instantiate(userButtonPrefab);
        item.transform.SetParent(userListPanel, false);

        // 绑定选择按钮
        Button btnSelect = item.transform.Find("BtnSelect").GetComponent<Button>();
        btnSelect.GetComponentInChildren<TextMeshProUGUI>().text = user;

        string capturedUser = user;
        btnSelect.onClick.AddListener(() => SelectUser(capturedUser));

        // 绑定删除按钮
        Button btnDelete = item.transform.Find("BtnDelete").GetComponent<Button>();
        btnDelete.onClick.AddListener(() =>
        {
            SaveManager.Instance.DeleteUser(capturedUser);
            RefreshUserList();
        });
    }
}
    // 选择已有用户
    public void SelectUser(string userName)
    {
        SaveManager.Instance.SetCurrentUser(userName);

        // 如果有存档，读取存档数据
        SaveData data = SaveManager.Instance.LoadGame(userName);
        if (data != null)
        {
            PlayerPrefs.SetInt("SelectedCharacter", data.selectedCharacter);
            // 好感度恢复在 GameManager 里处理
            PlayerPrefs.SetInt("LoadDialogueId", data.currentDialogueId);
            PlayerPrefs.SetInt("LoadFav1", data.favorability1);
            PlayerPrefs.SetInt("LoadFav2", data.favorability2);
            PlayerPrefs.SetInt("LoadFav3", data.favorability3);
        }

        SceneManager.LoadScene("HomePage");
    }

    // 创建新用户
    public void OnClickCreate()
    {
        string newUser = inputField.text.Trim();

        if (string.IsNullOrEmpty(newUser))
        {
            tipText.text = "用户名不能为空！";
            return;
        }

        if (newUser.Length > 10)
        {
            tipText.text = "用户名不能超过10个字！";
            return;
        }

        SaveManager.Instance.AddUserToList(newUser);
        SaveManager.Instance.SetCurrentUser(newUser);
        inputField.text = "";
        tipText.text = "";

        RefreshUserList();
        SceneManager.LoadScene("HomePage");
    }

    // 删除选中用户（简单版：删除输入框里的用户名）
    public void OnClickDelete()
    {
        string userName = inputField.text.Trim();
        if (string.IsNullOrEmpty(userName))
        {
            tipText.text = "请输入要删除的用户名！";
            return;
        }

        SaveManager.Instance.DeleteUser(userName);
        inputField.text = "";
        tipText.text = "已删除用户：" + userName;
        RefreshUserList();
    }
}