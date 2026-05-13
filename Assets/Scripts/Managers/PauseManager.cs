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
    public Button btnLoad;            // 新增：读档按钮（可选）
    public Button btnHome;
    public Button btnUserSelect;

    [Header("存档槽位面板")]
    public SaveSlotsPanel saveSlotsPanel;

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
        if (btnLoad != null) btnLoad.onClick.AddListener(OnClickLoad);
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
        if (saveSlotsPanel != null)
            saveSlotsPanel.Show(SaveSlotsPanel.Mode.Save);
    }

    void OnClickLoad()
    {
        if (saveSlotsPanel != null)
            saveSlotsPanel.Show(SaveSlotsPanel.Mode.Load);
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