using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomePageManager : MonoBehaviour
{
    public AudioSource bgmAudioSource;
    public Slider volumeSlider;

    void Start()
    {
        // 读取上次保存的音量，默认0.5
        float savedVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        volumeSlider.value = savedVolume;
        bgmAudioSource.volume = savedVolume;

        // 监听滑动条变化
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        bgmAudioSource.volume = value;
        // 保存音量设置
        PlayerPrefs.SetFloat("BGMVolume", value);
    }
    public void OnClickLinChenxi()
    {
        PlayerPrefs.SetInt("SelectedCharacter", 1);
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickGuYunshen()
    {
        PlayerPrefs.SetInt("SelectedCharacter", 2);
        SceneManager.LoadScene("GuScene");
    }

    public void OnClickXiaXinghe()
    {
        PlayerPrefs.SetInt("SelectedCharacter", 3);
        SceneManager.LoadScene("XiaScene");
    }

    public void OnClickQuit()
    {
        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();
    }
}