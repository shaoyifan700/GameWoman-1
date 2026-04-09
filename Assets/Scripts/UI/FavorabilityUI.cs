using UnityEngine;
using UnityEngine.UI;

public class FavorabilityUI : MonoBehaviour
{
    public Image[] hearts;          // 5个心形Image组件
    public Sprite heartFull;        // 实心红心
    public Sprite heartEmpty;       // 空心灰心
    public int characterId = 1;     // 当前角色ID

    void Start()
    {
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        if (GameManager.Instance == null) return;

        int favorability = GameManager.Instance.GetFavorability(characterId);
        int filledHearts = favorability / 20; // 每20点亮一颗心
        filledHearts = Mathf.Clamp(filledHearts, 0, 5);

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < filledHearts ? heartFull : heartEmpty;
        }
    }
}