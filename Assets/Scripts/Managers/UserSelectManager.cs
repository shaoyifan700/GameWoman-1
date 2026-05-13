using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;


public class UserSelectManager : MonoBehaviour
{
    public Image confirmBackground; // ConfirmBackground 的 Image 组件

    public Transform userListPanel;      // 用户列表容器
    public GameObject userButtonPrefab;  // 用户按钮预制体
    public TMP_InputField inputField;    // 用户名输入框
    public TMP_InputField passwordInput;        // 新建用户密码输入框
    public TMP_InputField confirmPasswordInput; // 新建用户确认密码输入框
    public Button btnTogglePwd;          // 新建密码-显示/隐藏按钮
    public Button btnToggleConfirmPwd;   // 新建确认密码-显示/隐藏按钮
    public TextMeshProUGUI tipText;      // 提示文字
    public GameObject confirmPanel;
    public TextMeshProUGUI confirmText;
    public Button btnConfirm;
    public Button btnCancel;

    [Header("登录密码弹窗")]
    public GameObject loginPanel;                // 登录密码弹窗
    public TextMeshProUGUI loginPromptText;      // "请输入xxx的密码"
    public TMP_InputField loginPasswordInput;    // 登录用密码框
    public Button btnToggleLoginPwd;             // 登录密码-显示/隐藏按钮
    public Button btnLoginConfirm;
    public Button btnLoginCancel;
    public TextMeshProUGUI loginTipText;         // 登录提示（密码错误等）

    private string pendingDeleteUser = ""; // 待删除的用户名
    private string pendingLoginUser  = ""; // 待登录的用户名

    void Start()
    {
        confirmPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);

        btnConfirm.onClick.AddListener(OnConfirmDelete);
        btnCancel.onClick.AddListener(OnCancelDelete);

        // 新建密码-显示/隐藏 切换
        if (btnTogglePwd != null && passwordInput != null)
            btnTogglePwd.onClick.AddListener(() => TogglePassword(passwordInput));
        if (btnToggleConfirmPwd != null && confirmPasswordInput != null)
            btnToggleConfirmPwd.onClick.AddListener(() => TogglePassword(confirmPasswordInput));

        // 登录弹窗按钮
        if (btnToggleLoginPwd != null && loginPasswordInput != null)
            btnToggleLoginPwd.onClick.AddListener(() => TogglePassword(loginPasswordInput));
        if (btnLoginConfirm != null) btnLoginConfirm.onClick.AddListener(OnConfirmLogin);
        if (btnLoginCancel  != null) btnLoginCancel.onClick.AddListener(OnCancelLogin);

        // 初始把所有密码框设成隐藏模式
        if (passwordInput != null)        passwordInput.contentType        = TMP_InputField.ContentType.Password;
        if (confirmPasswordInput != null) confirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (loginPasswordInput != null)   loginPasswordInput.contentType   = TMP_InputField.ContentType.Password;

        RefreshUserList();
    }

    // 切换密码框显示/隐藏
    void TogglePassword(TMP_InputField field)
    {
        if (field.contentType == TMP_InputField.ContentType.Password)
            field.contentType = TMP_InputField.ContentType.Standard;
        else
            field.contentType = TMP_InputField.ContentType.Password;
        field.ForceLabelUpdate();
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
    // 点击用户按钮，先弹出密码框
    public void SelectUser(string userName)
    {
        // 如果该用户没有设置过密码（老用户兼容），直接登录
        if (!SaveManager.Instance.HasPassword(userName))
        {
            DoLogin(userName);
            return;
        }

        // 否则弹出登录密码框
        pendingLoginUser = userName;
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (loginTipText != null) loginTipText.text = "";
        if (loginPromptText != null) loginPromptText.text = "请输入 " + userName + " 的密码";
        if (loginPanel != null)
        {
            loginPanel.transform.SetAsLastSibling();
            loginPanel.SetActive(true);
        }
    }

    // 确认密码后真正登录
    void OnConfirmLogin()
    {
        string pwd = loginPasswordInput != null ? loginPasswordInput.text : "";
        if (!SaveManager.Instance.VerifyPassword(pendingLoginUser, pwd))
        {
            if (loginTipText != null) loginTipText.text = "密码错误，请重试";
            return;
        }
        string user = pendingLoginUser;
        pendingLoginUser = "";
        if (loginPanel != null) loginPanel.SetActive(false);
        DoLogin(user);
    }

    void OnCancelLogin()
    {
        pendingLoginUser = "";
        if (loginPanel != null) loginPanel.SetActive(false);
    }

    // 真正的登录流程
    void DoLogin(string userName)
{
    SaveManager.Instance.SetCurrentUser(userName);
    if (StaminaManager.Instance != null) StaminaManager.Instance.InitStamina(userName);
    if (WalletManager.Instance != null) WalletManager.Instance.InitWallet(userName);

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

      // 1. 不能为空
    if (string.IsNullOrEmpty(newUser))
    {
        tipText.text = "用户名不能为空！";
        return;
    }

    // 2. 不能包含空格
   if (newUser.Contains(" "))
{
    tipText.text = "用户名不能包含空格！";
    return;
}

    // 3. 长度限制 2-10 个字符
    if (newUser.Length < 2 || newUser.Length > 10)
    {
        tipText.text = "用户名长度必须在2到10个字符之间！";
        return;
    }

    // 4. 只能是中文、英文、数字
    if (!Regex.IsMatch(newUser, @"^[\u4e00-\u9fa5a-zA-Z0-9]+$"))
    {
        tipText.text = "用户名只能包含中文、英文或数字！";
        return;
    }

    // 5. 不能纯数字
    if (Regex.IsMatch(newUser, @"^\d+$"))
    {
        tipText.text = "用户名不能为纯数字！";
        return;
    }

    // 6. 不能与已有用户重名
    string[] existingUsers = SaveManager.Instance.GetAllUsers();
    foreach (string existing in existingUsers)
    {
        if (!string.IsNullOrEmpty(existing) && existing == newUser)
        {
            tipText.text = "该用户名已存在，请尝试其他";
            return;
        }
    }

    // 7. 密码不能为空
    string pwd  = passwordInput        != null ? passwordInput.text        : "";
    string pwd2 = confirmPasswordInput != null ? confirmPasswordInput.text : "";
    if (string.IsNullOrEmpty(pwd))
    {
        tipText.text = "密码不能为空！";
        return;
    }

    // 8. 密码长度必须 ≥ 8
    if (pwd.Length < 8)
    {
        tipText.text = "密码长度不能少于8位！";
        return;
    }

    // 9. 两次输入的密码必须一致
    if (pwd != pwd2)
    {
        tipText.text = "两次输入的密码不一致！";
        return;
    }

    // 校验通过，保存密码 + 创建用户
    SaveManager.Instance.SetPassword(newUser, pwd);
    SaveManager.Instance.AddUserToList(newUser);
    SaveManager.Instance.SetCurrentUser(newUser);
    if (StaminaManager.Instance != null) StaminaManager.Instance.InitStamina(newUser);
    if (WalletManager.Instance != null) WalletManager.Instance.InitWallet(newUser);

    PlayerPrefs.SetInt("LoadDialogueId", 0);
    PlayerPrefs.SetInt("SavedCharacter", 0);
    PlayerPrefs.SetInt("LoadFav1", 0);
    PlayerPrefs.SetInt("LoadFav2", 0);
    PlayerPrefs.SetInt("LoadFav3", 0);
    PlayerPrefs.Save();

    inputField.text = "";
    if (passwordInput != null)        passwordInput.text = "";
    if (confirmPasswordInput != null) confirmPasswordInput.text = "";
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