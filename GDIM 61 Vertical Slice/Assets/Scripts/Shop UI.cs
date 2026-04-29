using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    const int CoffeeMachineCost = 30;

    void Awake() => Instance = this;

    public void OpenShop() => gameObject.SetActive(true);
    public void CloseShop() => gameObject.SetActive(false);

    public void BuyCoffeeMachine() => BuyCoffeeMachine(null, OrderType.Red);

    public void BuyCoffeeMachine(Sprite machineSprite, OrderType color)
    {
        if (OrderManager.Instance != null
            && OrderManager.Instance.TrySpendMoney(CoffeeMachineCost))
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
