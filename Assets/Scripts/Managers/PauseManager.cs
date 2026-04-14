using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{   
    public Button btnResume;
    public GameObject pausePanel;
    public TextMeshProUGUI userNameText;
    public Button btnSave;
    public Button btnHome;
    public Button btnUserSelect;

    private bool isPaused = false;
    private bool hasSaved = false;

    void Start()
    {
        pausePanel.SetActive(false);

        btnResume.onClick.AddListener(TogglePause);

        // 显示当前用户名
       if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.GetCurrentUser()))
            userNameText.text = "你好！" + SaveManager.Instance.GetCurrentUser();

        btnSave.onClick.AddListener(OnClickSave);
        btnHome.onClick.AddListener(OnClickHome);
        btnUserSelect.onClick.AddListener(OnClickUserSelect);
    }

    void Update()
    {
        // 按 Esc 切换暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 暂停按钮调用这个
    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }
   

    void OnClickSave()
{
    if (SaveManager.Instance == null)
    {
        Debug.LogError("SaveManager为空！");
        return;
    }

    int currentDialogueId = 0;
    DialogueManager dm = FindObjectOfType<DialogueManager>();
    if (dm != null)
        currentDialogueId = dm.GetCurrentDialogueId();

    Debug.Log("准备存档，当前对话ID：" + currentDialogueId);
    SaveManager.Instance.SaveGame(currentDialogueId);
    hasSaved = true; // 标记已存档


    userNameText.text = "存档成功！";
    Invoke("ResetUserNameText", 2f);
}

    void ResetUserNameText()
{
    if (SaveManager.Instance != null)
        userNameText.text = "你好！" + SaveManager.Instance.GetCurrentUser();
}

    void OnClickHome()
{
    Time.timeScale = 1;
    SceneManager.LoadScene("HomePage");
}


void OnClickUserSelect()
{
    Time.timeScale = 1;
    SceneManager.LoadScene("UserSelect");
}
}