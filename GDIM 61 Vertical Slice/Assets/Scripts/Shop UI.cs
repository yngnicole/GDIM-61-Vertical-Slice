using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    public const int RedMachineCost = 30;
    public const int BlueMachineCost = 50;
    public const int MuffinCost = 10;
    public const int CakeCost = 20;

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

    void Awake() => Instance = this;

    public void OpenShop() => gameObject.SetActive(true);
    public void CloseShop() => gameObject.SetActive(false);

    public void BuyCoffeeMachine() => BuyCoffeeMachine(null, OrderType.Red);

    public void BuyCoffeeMachine(Sprite machineSprite, OrderType color)
    {
        int cost = CostFor(color);
        if (OrderManager.Instance != null
            && OrderManager.Instance.TrySpendMoney(cost))
        {
            PlaceNewMachine(machineSprite, color);
        }
        CloseShop();
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
