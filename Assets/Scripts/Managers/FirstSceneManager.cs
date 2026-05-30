using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;   // 关键：必须 using 这个

public class FirstSceneManager : MonoBehaviour
{
    public void GoToUserSelect()
    {
                    SceneManager.LoadScene("UserSelect");

    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
