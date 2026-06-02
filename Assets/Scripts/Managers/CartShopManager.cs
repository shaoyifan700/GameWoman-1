using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 商店场景主控。展示商品 + 购物车 + 结算功能。
/// </summary>
public class CartShopManager : MonoBehaviour
{
    [Header("商品列表")]
    public Transform productGrid;            // 商品容器（Grid Layout Group）
    public GameObject productCardPrefab;     // 商品卡片预制体

    [Header("顶部栏")]
    public TextMeshProUGUI walletText;       // 余额显示
    public TextMeshProUGUI cartCountText;    // 购物车数量徽章
    public Button btnCart;                   // 打开购物车按钮
    public Button btnBack;                   // 返回HomePage

    [Header("购物车面板")]
    public GameObject cartPanel;
    public Transform cartItemContainer;      // 购物车条目容器
    public GameObject cartItemRowPrefab;
    public TextMeshProUGUI cartTotalText;
    public Button btnCheckout;
    public Button btnCloseCart;
    public TextMeshProUGUI cartTipText;      // 提示"余额不足"等

    private List<Product> products = new List<Product>();

    void Start()
    {
        LoadProducts();
        RefreshTopBar();

        if (cartPanel != null) cartPanel.SetActive(false);

        if (btnCart != null) btnCart.onClick.AddListener(OpenCart);
        if (btnBack != null) btnBack.onClick.AddListener(() => SceneManager.LoadScene("HomePage"));
        if (btnCheckout != null) btnCheckout.onClick.AddListener(Checkout);
        if (btnCloseCart != null) btnCloseCart.onClick.AddListener(() => cartPanel.SetActive(false));

        if (CartManager.Instance != null)
            CartManager.Instance.OnCartChanged += RefreshCartUI;

        BuildProductCards();
        RefreshCartUI();
    }

    void OnDestroy()
    {
        if (CartManager.Instance != null)
            CartManager.Instance.OnCartChanged -= RefreshCartUI;
    }

    // ─────────────────────────────────────────────
    // 商品加载与展示
    // ─────────────────────────────────────────────

    void LoadProducts()
    {
        string path = Application.streamingAssetsPath + "/Data/products.json";
        if (!File.Exists(path)) { Debug.LogError("找不到 products.json"); return; }

        string json = File.ReadAllText(path);
        ProductWrapper w = JsonUtility.FromJson<ProductWrapper>(json);
        if (w != null && w.products != null) products = w.products;
        Debug.Log("商品加载完成，共 " + products.Count + " 种");
    }

    void BuildProductCards()
    {
        if (productGrid == null || productCardPrefab == null) return;

        foreach (Transform t in productGrid) Destroy(t.gameObject);

        foreach (var p in products)
        {
            GameObject card = Instantiate(productCardPrefab, productGrid);
            var nameText = FindChild<TextMeshProUGUI>(card, "NameText");
            var descText = FindChild<TextMeshProUGUI>(card, "DescText");
            var priceText = FindChild<TextMeshProUGUI>(card, "PriceText");
            var addBtn = FindChild<Button>(card, "BtnAdd");

            if (nameText != null) nameText.text = p.name;
            if (descText != null) descText.text = p.description;
            if (priceText != null) priceText.text = p.price + " 元";

            if (addBtn != null)
            {
                string capturedId = p.id;
                addBtn.onClick.AddListener(() => {
                    if (CartManager.Instance == null) return;
                    CartManager.Instance.AddItem(capturedId, 1);
                });
            }
        }
    }

    // ─────────────────────────────────────────────
    // 购物车UI
    // ─────────────────────────────────────────────

    void OpenCart()
    {
        cartPanel.SetActive(true);
        cartPanel.transform.SetAsLastSibling();
        RefreshCartUI();
    }

    void RefreshCartUI()
    {
        // 顶部购物车数量
        if (cartCountText != null && CartManager.Instance != null)
        {
            int n = CartManager.Instance.GetTotalItemCount();
            cartCountText.text = n.ToString();
            cartCountText.gameObject.SetActive(n > 0);
        }

        // 购物车面板内容
        if (cartItemContainer == null) return;
        if (CartManager.Instance == null) return;

        foreach (Transform t in cartItemContainer) Destroy(t.gameObject);

        int total = 0;
        foreach (var kv in CartManager.Instance.GetAllItems())
        {
            Product p = products.Find(x => x.id == kv.Key);
            if (p == null) continue;

            int subtotal = p.price * kv.Value;
            total += subtotal;

            if (cartItemRowPrefab != null)
            {
                GameObject row = Instantiate(cartItemRowPrefab, cartItemContainer);
                var nameText = FindChild<TextMeshProUGUI>(row, "NameText");
                var qtyText  = FindChild<TextMeshProUGUI>(row, "QtyText");
                var subTotalText = FindChild<TextMeshProUGUI>(row, "SubtotalText");
                var btnMinus = FindChild<Button>(row, "BtnMinus");
                var btnPlus  = FindChild<Button>(row, "BtnPlus");
                var btnDel   = FindChild<Button>(row, "BtnDelete");

                // 诊断日志：检查按钮是否被找到
                Debug.Log($"[购物车行] {p.name}  btnMinus={(btnMinus!=null)} btnPlus={(btnPlus!=null)} btnDel={(btnDel!=null)}");

                if (nameText != null) nameText.text = p.name;
                if (qtyText != null)  qtyText.text  = kv.Value.ToString();
                if (subTotalText != null) subTotalText.text = subtotal + " 元";

                string capturedId = p.id;
                if (btnMinus != null) btnMinus.onClick.AddListener(() => {
                    Debug.Log("点击 - " + capturedId);
                    CartManager.Instance.AddItem(capturedId, -1);
                });
                if (btnPlus  != null) btnPlus.onClick.AddListener(() => {
                    Debug.Log("点击 + " + capturedId);
                    CartManager.Instance.AddItem(capturedId, 1);
                });
                if (btnDel   != null) btnDel.onClick.AddListener(() => {
                    Debug.Log("点击 删除 " + capturedId);
                    CartManager.Instance.RemoveItem(capturedId);
                });
            }
        }

        if (cartTotalText != null) cartTotalText.text = "总计：" + total + " 元";
        if (cartTipText != null) cartTipText.text = "";
    }

    // ─────────────────────────────────────────────
    // 结算
    // ─────────────────────────────────────────────

    void Checkout()
    {
        if (CartManager.Instance == null) return;

        int total = 0;
        foreach (var kv in CartManager.Instance.GetAllItems())
        {
            Product p = products.Find(x => x.id == kv.Key);
            if (p != null) total += p.price * kv.Value;
        }

        if (total <= 0)
        {
            if (cartTipText != null) cartTipText.text = "购物车是空的！";
            return;
        }

        float balance = WalletManager.Instance.GetBalance();
        if (balance < total)
        {
            if (cartTipText != null) cartTipText.text = "余额不足！";
            return;
        }

        WalletManager.Instance.Spend(total);

        foreach (var kv in CartManager.Instance.GetAllItems())
        {
            Product p = products.Find(x => x.id == kv.Key);
            if (p == null) continue;
            for (int i = 0; i < kv.Value; i++) ApplyProductEffect(p);
        }

        CartManager.Instance.Clear();
        if (cartTipText != null) cartTipText.text = "购买成功！";
        RefreshTopBar();
        RefreshCartUI();
    }

    void ApplyProductEffect(Product p)
    {
        switch (p.category)
        {
            case "ticket":
                if (BalloonTicketsManager.Instance != null)
                    BalloonTicketsManager.Instance.AddTickets(p.value);
                break;
            case "stamina":
                if (StaminaManager.Instance != null)
                    StaminaManager.Instance.AddStamina(p.value);
                break;
        }
    }

    void RefreshTopBar()
    {
        if (walletText != null && WalletManager.Instance != null)
            walletText.text = "金币：" + WalletManager.Instance.GetBalance().ToString("0");
    }

    static T FindChild<T>(GameObject root, string childName) where T : Component
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t.GetComponent<T>();
        return null;
    }
}
