using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    public const int RedMachineCost = 30;
    public const int BlueMachineCost = 50;
    public const int MuffinCost = 10;
    public const int CakeCost = 20;

    public const string BlueMachineId = "blue machine";
    public const string RedMachineId  = "red machine";
    public const string MuffinId      = "muffin";
    public const string CakeId        = "cake";

    static readonly HashSet<string> _purchased = new HashSet<string>();

    public static bool IsPurchased(string itemId)
        => !string.IsNullOrEmpty(itemId) && _purchased.Contains(itemId);

    public static void MarkPurchased(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId)) _purchased.Add(itemId);
    }

    public static int CostFor(OrderType color)
        => color == OrderType.Blue ? BlueMachineCost : RedMachineCost;

    // Identifier can be a GameObject name or a sprite name (e.g. "coffee_pot_v2").
    public static int CostFor(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return 0;
        string n = identifier.ToLower();
        if (n.Contains("coffee machine") || n.Contains("coffee_machine")
            || n.Contains("coffee pot") || n.Contains("coffee_pot"))
        {
            bool blue = n.Contains("blue") || n.Contains("pot");
            return blue ? BlueMachineCost : RedMachineCost;
        }
        if (n.Contains("muffin")) return MuffinCost;
        if (n.Contains("cake"))   return CakeCost;
        return 0;
    }

    // Maps a GameObject/sprite name onto a canonical item id used by the
    // purchased-registry and price tags.
    public static string IdFor(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return null;
        string n = identifier.ToLower();
        if (n.Contains("coffee machine") || n.Contains("coffee_machine")
            || n.Contains("coffee pot") || n.Contains("coffee_pot"))
            return (n.Contains("blue") || n.Contains("pot")) ? BlueMachineId : RedMachineId;
        if (n.Contains("muffin")) return MuffinId;
        if (n.Contains("cake"))   return CakeId;
        return null;
    }

    void Awake() => Instance = this;

    public void OpenShop() => gameObject.SetActive(true);
    public void CloseShop() => gameObject.SetActive(false);

    public void BuyCoffeeMachine() => BuyCoffeeMachine(null, OrderType.Red);

    public void BuyCoffeeMachine(Sprite machineSprite, OrderType color)
    {
        string id = color == OrderType.Blue ? BlueMachineId : RedMachineId;
        if (IsPurchased(id)) { CloseShop(); return; }

        int cost = CostFor(color);
        if (OrderManager.Instance != null
            && OrderManager.Instance.TrySpendMoney(cost))
        {
            PlaceNewMachine(machineSprite, color);
            MarkPurchased(id);
        }
        CloseShop();
    }

    public void BuyFood(Sprite foodSprite, OrderType foodType)
    {
        if (foodType != OrderType.Muffin && foodType != OrderType.Cake)
        {
            Debug.LogWarning("[ShopUI] BuyFood called with non-food type: " + foodType);
            CloseShop();
            return;
        }

        string id = foodType == OrderType.Muffin ? MuffinId : CakeId;
        if (IsPurchased(id)) { CloseShop(); return; }

        int cost = foodType == OrderType.Muffin ? MuffinCost : CakeCost;
        if (OrderManager.Instance != null
            && OrderManager.Instance.TrySpendMoney(cost))
        {
            PlaceNewFoodItem(foodSprite, foodType);
            MarkPurchased(id);
        }
        CloseShop();
    }

    void PlaceNewFoodItem(Sprite foodSprite, OrderType foodType)
    {
        // Anchor placement to the existing coffee machine so food items
        // sit on the same counter line. Offsets keep muffin/cake distinct.
        CoffeeMachine anchor = Object.FindObjectOfType<CoffeeMachine>();
        if (anchor == null)
        {
            Debug.LogWarning("[ShopUI] No anchor (CoffeeMachine) to place food next to");
            return;
        }

        // Muffin gets a lower z than cake so it renders in front when they overlap.
        Vector3 offset = foodType == OrderType.Muffin
            ? new Vector3(-2.25f, -1f, -0.1f)
            : new Vector3(-3f, -0.6f, 0f);

        GameObject go = new GameObject(foodType + "Display");
        go.transform.position = anchor.transform.position + offset;
        // Match the cafe's scale so the food sprite renders at the same size
        // as the existing coffee machines on the counter.
        go.transform.localScale = anchor.transform.lossyScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = foodSprite;
        sr.sortingOrder = 10;

        if (foodSprite != null)
        {
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = foodSprite.bounds.size;
            col.offset = foodSprite.bounds.center;
        }

        FoodItem food = go.AddComponent<FoodItem>();
        food.Configure(foodType, foodSprite);

        Debug.Log("[ShopUI] Placed " + foodType + " display at " + go.transform.position);
    }

    void PlaceNewMachine(Sprite machineSprite, OrderType color)
    {
        CoffeeMachine existing = Object.FindObjectOfType<CoffeeMachine>();
        if (existing == null) return;

        GameObject clone = Instantiate(existing.gameObject);
        CoffeeMachine clonedCm = clone.GetComponent<CoffeeMachine>();
        if (clonedCm != null) clonedCm.ResetToIdle();

        SpriteRenderer cloneSr = clone.GetComponent<SpriteRenderer>()
                              ?? clone.GetComponentInChildren<SpriteRenderer>();
        if (machineSprite != null && cloneSr != null)
            cloneSr.sprite = machineSprite;

        clone.transform.position = existing.transform.position + new Vector3(-0.75f, 0.375f, 0f);

        CoffeeMachine cm = clone.GetComponent<CoffeeMachine>();
        if (cm == null) cm = clone.AddComponent<CoffeeMachine>();
        cm.SetMachineColor(color);

        BoxCollider2D existingCol = clone.GetComponent<BoxCollider2D>();
        if (existingCol == null && cloneSr != null && cloneSr.sprite != null)
        {
            BoxCollider2D col = clone.AddComponent<BoxCollider2D>();
            col.size = cloneSr.sprite.bounds.size;
            col.offset = cloneSr.sprite.bounds.center;
        }
        else if (existingCol != null && machineSprite != null && cloneSr != null && cloneSr.sprite != null)
        {
            existingCol.size = cloneSr.sprite.bounds.size;
            existingCol.offset = cloneSr.sprite.bounds.center;
        }
    }
}
