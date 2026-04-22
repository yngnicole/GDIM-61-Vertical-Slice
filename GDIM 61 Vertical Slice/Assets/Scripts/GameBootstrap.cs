using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto-configures the scene at runtime. No manual setup needed.
/// Runs automatically after the scene loads.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        Debug.Log("[Bootstrap] Setting up scene...");
        RemovePreplacedObjects();
        SetupOrderManager();
        SetupCoffeeMachines();
        SetupUI();
        SetupAutoTester();
        SetupShop();
        Debug.Log("[Bootstrap] Setup complete! Press T to run automated test.");
    }

    static void RemovePreplacedObjects()
    {
        // Remove the pre-placed static NPC and its container
        foreach (NPC npc in Object.FindObjectsOfType<NPC>())
        {
            Debug.Log("[Bootstrap] Removing pre-placed NPC: " + npc.gameObject.name);
            if (npc.transform.root != npc.transform)
                Object.Destroy(npc.transform.root.gameObject);
            else
                Object.Destroy(npc.gameObject);
        }

        // Remove leftover "GDIM 61 coffee" icon sprites near the old NPC.
        // These are decorative/bubble icons, NOT the actual coffee machine.
        // The real machine is named "coffee_machine_ase".
        foreach (SpriteRenderer sr in Object.FindObjectsOfType<SpriteRenderer>())
        {
            string name = sr.gameObject.name.ToLower();
            if (name.Contains("gdim") && name.Contains("coffee"))
            {
                Debug.Log("[Bootstrap] Removing leftover icon: " + sr.gameObject.name);
                Object.Destroy(sr.gameObject);
            }
        }
    }

    static void SetupOrderManager()
    {
        if (Object.FindObjectOfType<OrderManager>() != null) return;

        GameObject go = new GameObject("OrderManager");
        go.AddComponent<OrderManager>();
        Debug.Log("[Bootstrap] Created OrderManager");
    }

    static void SetupCoffeeMachines()
    {
        int count = 0;
        foreach (SpriteRenderer sr in Object.FindObjectsOfType<SpriteRenderer>())
        {
            if (!IsCoffeeMachineObject(sr.transform)) continue;
            if (sr.GetComponentInParent<NPC>() != null) continue;
            if (sr.GetComponent<CoffeeMachine>() != null) continue;

            BoxCollider2D col = sr.gameObject.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = sr.gameObject.AddComponent<BoxCollider2D>();
                if (sr.sprite != null)
                    col.size = sr.sprite.bounds.size;
            }

            sr.gameObject.AddComponent<CoffeeMachine>();
            count++;
            Debug.Log("[Bootstrap] Added CoffeeMachine to: " + sr.gameObject.name
                + " at " + sr.transform.position);
        }
        Debug.Log("[Bootstrap] Set up " + count + " coffee machines");
    }

    static bool IsCoffeeMachineObject(Transform t)
    {
        // Only match actual machines ("coffee_machine_ase"), not icon sprites ("GDIM 61 coffee")
        Transform current = t;
        while (current != null)
        {
            if (current.gameObject.name.ToLower().Contains("coffee_machine"))
                return true;
            current = current.parent;
        }
        return false;
    }

    static void SetupUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Bootstrap] No Canvas found for UI");
            return;
        }

        OrderManager mgr = Object.FindObjectOfType<OrderManager>();
        if (mgr == null) return;

        // Find a usable font - try multiple approaches
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Helvetica", 24);

        if (font == null)
        {
            // Last resort: grab any available OS font
            string[] fontNames = Font.GetOSInstalledFontNames();
            if (fontNames.Length > 0)
                font = Font.CreateDynamicFontFromOSFont(fontNames[0], 24);
        }

        // Dedicated HUD canvas — Screen Space Overlay guarantees it renders above everything
        GameObject hudGo = new GameObject("HUDCanvas");
        Canvas hudCanvas = hudGo.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 10;
        hudGo.AddComponent<CanvasScaler>();
        hudGo.AddComponent<GraphicRaycaster>();

        // Status text (bottom-center, shifted left to make room for money)
        Text statusText = CreateText(hudGo.transform, "StatusText",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-100, 60), new Vector2(420, 60),
            28, TextAnchor.UpperCenter, font);
        statusText.text = "Waiting for NPC...";
        mgr.SetStatusText(statusText);

        // Money text (bottom, to the right of status text)
        Text moneyText = CreateText(hudGo.transform, "MoneyText",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(190, 60), new Vector2(160, 60),
            28, TextAnchor.UpperCenter, font);
        moneyText.text = "$0";
        mgr.SetMoneyText(moneyText);

        // Move shop icon to HUD canvas so it's always visible at bottom-right
        GameObject shopIcon = GameObject.Find("Shop icon");
        if (shopIcon != null)
        {
            shopIcon.transform.SetParent(hudGo.transform, false);
            RectTransform rt = shopIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                rt.anchoredPosition = new Vector2(-400, 100);
            }
        }

        Debug.Log("[Bootstrap] UI created. Font: " + (font != null ? font.name : "NONE"));
    }

    static void SetupShop()
    {
        // Find the Shop Canvas (inactive by default)
        Canvas[] allCanvases = Object.FindObjectsOfType<Canvas>(true);
        GameObject shopCanvasGo = null;
        foreach (Canvas c in allCanvases)
            if (c.gameObject.name == "Shop Canvas") { shopCanvasGo = c.gameObject; break; }

        if (shopCanvasGo == null) { Debug.LogWarning("[Bootstrap] Shop Canvas not found"); return; }

        // Add ShopUI component
        ShopUI shopUI = shopCanvasGo.GetComponent<ShopUI>() ?? shopCanvasGo.AddComponent<ShopUI>();

        // Wire up shop icon button → open shop
        GameObject shopIconGo = GameObject.Find("Shop icon");
        if (shopIconGo != null)
        {
            Button shopBtn = shopIconGo.GetComponent<Button>();
            if (shopBtn != null)
            {
                shopBtn.onClick.RemoveAllListeners();
                shopBtn.onClick.AddListener(shopUI.OpenShop);
            }
        }

        // Wire up X button → close shop
        Button xBtn = FindButtonInCanvas(shopCanvasGo, "X button");
        if (xBtn != null)
        {
            xBtn.onClick.RemoveAllListeners();
            xBtn.onClick.AddListener(shopUI.CloseShop);
        }

        // Wire up the Buy button → buy machine
        Button buyBtn = FindButtonInCanvas(shopCanvasGo, "Buy ");
        if (buyBtn != null)
        {
            buyBtn.onClick.RemoveAllListeners();
            buyBtn.onClick.AddListener(shopUI.BuyCoffeeMachine);
        }

        // Also make the coffee pot image itself clickable (it's what the user sees and clicks)
        Transform coffeePot = FindChildByName(shopCanvasGo.transform, "coffee pot");
        if (coffeePot != null)
        {
            Button potBtn = coffeePot.GetComponent<Button>() ?? coffeePot.gameObject.AddComponent<Button>();
            UnityEngine.UI.Image img = coffeePot.GetComponent<UnityEngine.UI.Image>();
            if (img != null) potBtn.targetGraphic = img;
            potBtn.onClick.RemoveAllListeners();
            potBtn.onClick.AddListener(shopUI.BuyCoffeeMachine);
        }
    }

    static Button FindButtonInCanvas(GameObject canvasGo, string name)
    {
        foreach (Button btn in canvasGo.GetComponentsInChildren<Button>(true))
            if (btn.gameObject.name == name) return btn;
        return null;
    }

    static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    static void SetupAutoTester()
    {
        if (Object.FindObjectOfType<AutoTester>() != null) return;

        GameObject go = new GameObject("AutoTester");
        go.AddComponent<AutoTester>();
        Debug.Log("[Bootstrap] AutoTester created. Press T to run automated test.");
    }

    static Text CreateText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        int fontSize, TextAnchor alignment, Font font)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text text = obj.AddComponent<Text>();
        text.fontSize = fontSize;
        text.color = Color.yellow;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;
        if (font != null) text.font = font;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return text;
    }
}
