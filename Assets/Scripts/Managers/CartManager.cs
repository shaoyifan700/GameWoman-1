using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 购物车（运行时数据，不持久化）。
/// 跨场景保留：玩家进商店选了东西，可以回主菜单再回来，购物车不丢。
/// 数量任意，结算时才检查余额。
/// </summary>
public class CartManager : MonoBehaviour
{
    public static CartManager Instance;

    private Dictionary<string, int> items = new Dictionary<string, int>();  // productId -> quantity

    public System.Action OnCartChanged;  // 购物车变化时通知UI刷新

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void AddItem(string productId, int delta = 1)
    {
        if (!items.ContainsKey(productId)) items[productId] = 0;
        items[productId] += delta;
        if (items[productId] <= 0) items.Remove(productId);
        OnCartChanged?.Invoke();
    }

    public void RemoveItem(string productId)
    {
        if (items.ContainsKey(productId)) items.Remove(productId);
        OnCartChanged?.Invoke();
    }

    public void SetQuantity(string productId, int qty)
    {
        if (qty <= 0) items.Remove(productId);
        else items[productId] = qty;
        OnCartChanged?.Invoke();
    }

    public int GetQuantity(string productId)
    {
        return items.ContainsKey(productId) ? items[productId] : 0;
    }

    public Dictionary<string, int> GetAllItems() { return new Dictionary<string, int>(items); }

    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (var v in items.Values) total += v;
        return total;
    }

    public void Clear()
    {
        items.Clear();
        OnCartChanged?.Invoke();
    }
}
