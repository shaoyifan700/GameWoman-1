using UnityEngine;
using UnityEngine.SceneManagement;

public class HomePageManager : MonoBehaviour
{
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
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}