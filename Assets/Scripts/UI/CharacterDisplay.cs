using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplay : MonoBehaviour
{
    public Image characterImage;

    // 根据角色ID和表情显示立绘
    public void ShowCharacter(int characterId, string expression)
{
    if (characterId == 0)
    {
        characterImage.gameObject.SetActive(false);
        return;
    }

    string expression_str = string.IsNullOrEmpty(expression) ? "Normal" : expression;
    string path = "Characters/Boy_0" + characterId + "_" + expression_str;
    
    Debug.Log("加载立绘路径：" + path); // 加这行
    
    Sprite sprite = Resources.Load<Sprite>(path);
    if (sprite != null)
    {
        characterImage.gameObject.SetActive(true);
        characterImage.sprite = sprite;
    }
    else
    {
        characterImage.gameObject.SetActive(false);
        Debug.LogWarning("找不到立绘：" + path);
    }
}

    public void HideCharacter()
    {
        characterImage.gameObject.SetActive(false);
    }
}