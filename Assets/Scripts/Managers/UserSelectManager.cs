using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UserSelectManager : MonoBehaviour
{
    public Image confirmBackground; // ConfirmBackground 的 Image 组件

    public Transform userListPanel;      // 用户列表容器
    public GameObject userButtonPrefab;  // 用户按钮预制体
    public TMP_InputField inputField;    // 输入框
    public TextMeshProUGUI tipText;      // 提示文字
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public Button btnConfirm;
    public Button btnCancel;

    private string pendingDeleteUser = ""; // 待删除的用户名

    void Start()
    {
        confirmPanel.SetActive(false);
    btnConfirm.onClick.AddListener(OnConfirmDelete);
    btnCancel.onClick.AddListener(OnCancelDelete);
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
        btnDelete.onClick.AddListener(() => ShowConfirmPanel(capturedUser));

        
    }
}
    // 选择已有用户
    public void SelectUser(string userName)
{
    SaveManager.Instance.SetCurrentUser(userName);

    SaveData data = SaveManager.Instance.LoadGame(userName);
    if (data != null)
    {
        // 有存档，读取存档数据
        PlayerPrefs.SetInt("SelectedCharacter", data.selectedCharacter);
        PlayerPrefs.SetInt("LoadDialogueId", data.currentDialogueId);
        PlayerPrefs.SetInt("SavedCharacter", data.selectedCharacter);
        PlayerPrefs.SetInt("LoadFav1", data.favorability1);
        PlayerPrefs.SetInt("LoadFav2", data.favorability2);
        PlayerPrefs.SetInt("LoadFav3", data.favorability3);
    }
    else
    {
        // 没有存档，从头开始
        PlayerPrefs.SetInt("LoadDialogueId", 0);
        PlayerPrefs.SetInt("SavedCharacter", 0);
        PlayerPrefs.SetInt("LoadFav1", 0);
        PlayerPrefs.SetInt("LoadFav2", 0);
        PlayerPrefs.SetInt("LoadFav3", 0);
    }
    PlayerPrefs.Save();

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

    // 新用户清除所有存档读取标记，从头开始
    PlayerPrefs.SetInt("LoadDialogueId", 0);
    PlayerPrefs.SetInt("SavedCharacter", 0);
    PlayerPrefs.SetInt("LoadFav1", 0);
    PlayerPrefs.SetInt("LoadFav2", 0);
    PlayerPrefs.SetInt("LoadFav3", 0);
    PlayerPrefs.Save();

    inputField.text = "";
    tipText.text = "";

    RefreshUserList();
    SceneManager.LoadScene("HomePage");
}
    

    // 删除选中用户（简单版：删除输入框里的用户名）
    void ShowConfirmPanel(string userName)
{
    pendingDeleteUser = userName;
    confirmText.text = "您确定删除用户：" + userName + "？";
    confirmPanel.SetActive(true);
}

void OnConfirmDelete()
{
    if (!string.IsNullOrEmpty(pendingDeleteUser))
    {
        SaveManager.Instance.DeleteUser(pendingDeleteUser);
        pendingDeleteUser = "";
    }
    confirmPanel.SetActive(false);
    RefreshUserList();
}

void OnCancelDelete()
{
    pendingDeleteUser = "";
    confirmPanel.SetActive(false);
}

// 点击遮罩区域（弹窗外）
    public void OnClickOverlay()
    {
        StartCoroutine(ShakeAndRed());
    }
     // 抖动 + 变红效果
    IEnumerator ShakeAndRed()
    {
        // 变红
        confirmBackground.color = new Color(1f, 0.3f, 0.3f, 1f);

        // 抖动两下
        Vector3 originalPos = confirmBackground.rectTransform.localPosition;
        float shakeDist = 10f;
        float shakeSpeed = 0.05f;

        confirmBackground.rectTransform.localPosition = originalPos + new Vector3(shakeDist, 0, 0);
        yield return new WaitForSeconds(shakeSpeed);
        confirmBackground.rectTransform.localPosition = originalPos + new Vector3(-shakeDist, 0, 0);
        yield return new WaitForSeconds(shakeSpeed);
        confirmBackground.rectTransform.localPosition = originalPos + new Vector3(shakeDist, 0, 0);
        yield return new WaitForSeconds(shakeSpeed);
        confirmBackground.rectTransform.localPosition = originalPos + new Vector3(-shakeDist, 0, 0);
        yield return new WaitForSeconds(shakeSpeed);

        // 还原位置
        confirmBackground.rectTransform.localPosition = originalPos;

        // 颜色慢慢恢复白色
        float duration = 0.5f;
        float elapsed = 0f;
        Color redColor = new Color(1f, 0.3f, 0.3f, 1f);
        Color normalColor = Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            confirmBackground.color = Color.Lerp(redColor, normalColor, elapsed / duration);
            yield return null;
        }

        confirmBackground.color = normalColor;
    }
}