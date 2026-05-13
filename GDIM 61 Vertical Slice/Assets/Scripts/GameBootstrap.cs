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
        MarkExistingMachinesPurchased();
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
                {
                    col.size = sr.sprite.bounds.size;
                    col.offset = sr.sprite.bounds.center;
                }
            }

            CoffeeMachine cm = sr.gameObject.AddComponent<CoffeeMachine>();
            cm.SetMachineColor(InferMachineColor(sr));
            count++;
            Debug.Log("[Bootstrap] Added CoffeeMachine to: " + sr.gameObject.name
                + " at " + sr.transform.position);
        }
        Debug.Log("[Bootstrap] Set up " + count + " coffee machines");
    }

    static void MarkExistingMachinesPurchased()
    {
        // Any machine already in the scene at start counts as "owned" so the
        // shop won't let the player buy a duplicate of its color.
        foreach (CoffeeMachine cm in Object.FindObjectsOfType<CoffeeMachine>())
        {
            string id = cm.MachineColor == OrderType.Blue
                ? ShopUI.BlueMachineId : ShopUI.RedMachineId;
            ShopUI.MarkPurchased(id);
            Debug.Log("[Bootstrap] Marked '" + id + "' as already owned (starter machine)");
        }
    }

    static OrderType InferMachineColor(SpriteRenderer sr)
    {
        // The shop's blue prefab uses coffee_pot_v2; the original red one uses coffee_machine_ase.
        string spriteName = (sr != null && sr.sprite != null) ? sr.sprite.name.ToLower() : "";
        string objName = sr != null ? sr.gameObject.name.ToLower() : "";
        if (spriteName.Contains("pot") || spriteName.Contains("blue") || objName.Contains("blue"))
            return OrderType.Blue;
        return OrderType.Red;
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

        // Top-right corner; pivot top-right so it grows leftward when a delta is shown.
        Text moneyText = CreateText(hudGo.transform, "MoneyText",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(420, 60),
            28, TextAnchor.UpperRight, font);
        moneyText.supportRichText = true;
        // The text overlaps the bottom row of NPCs in screen space — disable raycasts
        // so it doesn't intercept world clicks. Only the shop icon needs to be clickable.
        moneyText.raycastTarget = false;
        mgr.SetMoneyText(moneyText);

        // Move shop icon to HUD canvas, true bottom-right corner, just right of the status text.
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
                rt.sizeDelta = new Vector2(60, 60);
                rt.anchoredPosition = new Vector2(-20, 60);
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

        // Scale shop contents via a wrapper so we don't have to touch every
        // child's anchoredPosition + sizeDelta individually.
        ScaleShopContents(shopCanvasGo, 0.75f);

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

        // Wire every shop item (machines, muffin, cake, …) — add a price tag,
        // and a buy handler for the ones we know how to "use".
        WireShopItems(shopCanvasGo, shopUI);
    }

    static void ScaleShopContents(GameObject shopCanvasGo, float scale)
    {
        RectTransform shopRt = shopCanvasGo.GetComponent<RectTransform>();
        if (shopRt == null) return;
        if (shopRt.Find("ContentWrapper") != null) return; // idempotent

        // Snapshot the existing children before reparenting.
        var existing = new System.Collections.Generic.List<RectTransform>();
        for (int i = 0; i < shopRt.childCount; i++)
            existing.Add(shopRt.GetChild(i) as RectTransform);

        GameObject wrapperGo = new GameObject("ContentWrapper", typeof(RectTransform));
        RectTransform wrapper = wrapperGo.GetComponent<RectTransform>();
        wrapper.SetParent(shopRt, false);
        wrapper.anchorMin = new Vector2(0.5f, 0.5f);
        wrapper.anchorMax = new Vector2(0.5f, 0.5f);
        wrapper.pivot = new Vector2(0.5f, 0.5f);
        wrapper.anchoredPosition = Vector2.zero;
        wrapper.sizeDelta = Vector2.zero;
        wrapper.localScale = new Vector3(scale, scale, 1f);

        foreach (RectTransform child in existing)
            if (child != null) child.SetParent(wrapper, false);
    }

    static void WireShopItems(GameObject shopCanvasGo, ShopUI shopUI)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        foreach (UnityEngine.UI.Image img in shopCanvasGo.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            // Identify by GameObject name first, then fall back to the sprite name
            // — the items inside the slot boxes are often unnamed "Image" objects.
            string objName = img.gameObject.name;
            string spriteName = img.sprite != null ? img.sprite.name : "";
            string itemId = ShopUI.IdFor(objName) ?? ShopUI.IdFor(spriteName);
            int cost = ShopUI.CostFor(objName);
            if (cost == 0) cost = ShopUI.CostFor(spriteName);
            if (cost == 0 || itemId == null) continue;

            string lower = (objName + " " + spriteName).ToLower();
            bool isMachine = lower.Contains("coffee machine") || lower.Contains("coffee_machine")
                          || lower.Contains("coffee pot") || lower.Contains("coffee_pot");
            Sprite sprite = img.sprite;

            Button btn = img.GetComponent<Button>() ?? img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.RemoveAllListeners();

            if (isMachine)
            {
                OrderType color = (lower.Contains("blue") || lower.Contains("pot"))
                    ? OrderType.Blue : OrderType.Red;
                btn.onClick.AddListener(() => shopUI.BuyCoffeeMachine(sprite, color));
            }
            else if (itemId == ShopUI.MuffinId)
            {
                btn.onClick.AddListener(() => shopUI.BuyFood(sprite, OrderType.Muffin));
            }
            else if (itemId == ShopUI.CakeId)
            {
                btn.onClick.AddListener(() => shopUI.BuyFood(sprite, OrderType.Cake));
            }

            AddPriceTag(img.gameObject, itemId, cost, font);

            Debug.Log("[Bootstrap] Wired shop item: " + objName
                + " (sprite=" + spriteName + ", id=" + itemId + ") cost=$" + cost);
        }
    }

    static void AddPriceTag(GameObject iconGo, string itemId, int cost, Font font)
    {
        Transform existing = iconGo.transform.Find("PriceTag");
        if (existing != null) Object.Destroy(existing.gameObject);

        GameObject tagGo = new GameObject("PriceTag", typeof(RectTransform));
        tagGo.transform.SetParent(iconGo.transform, false);

        // Anchored to bottom-center of the icon, sitting just below it.
        RectTransform rt = tagGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -4f);
        rt.sizeDelta = new Vector2(120f, 32f);

        Text label = tagGo.AddComponent<Text>();
        label.text = "$" + cost;
        label.alignment = TextAnchor.UpperCenter;
        label.fontSize = 20;
        label.fontStyle = FontStyle.Bold;
        if (font != null) label.font = font;
        label.raycastTarget = false; // don't intercept the icon's button clicks

        Outline outline = tagGo.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // PriceTagUpdater paints the label green/red based on current money,
        // or "exists" in red once the item has been purchased.
        PriceTagUpdater updater = tagGo.AddComponent<PriceTagUpdater>();
        updater.Init(itemId, cost, label);
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
