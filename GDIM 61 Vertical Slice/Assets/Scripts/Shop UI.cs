using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    const int CoffeeMachineCost = 30;

    void Awake() => Instance = this;

    public void OpenShop() => gameObject.SetActive(true);
    public void CloseShop() => gameObject.SetActive(false);

    public void BuyCoffeeMachine()
    {
        if (OrderManager.Instance == null) return;
        if (!OrderManager.Instance.TrySpendMoney(CoffeeMachineCost)) return;

        PlaceNewMachine();
        CloseShop();
    }

    void PlaceNewMachine()
    {
        CoffeeMachine existing = Object.FindObjectOfType<CoffeeMachine>();
        if (existing == null) return;

        // Clone the existing machine so visuals and sorting match exactly,
        // then immediately wipe any runtime state (color, brew flags, icons)
        GameObject clone = Instantiate(existing.gameObject);
        CoffeeMachine clonedCm = clone.GetComponent<CoffeeMachine>();
        if (clonedCm != null) clonedCm.ResetToIdle();

        // Place it offset from the existing machine along the counter
        clone.transform.position = existing.transform.position + new Vector3(-1.5f, 0.75f, 0f);

        // Ensure a fresh CoffeeMachine component (no brewing state carried over)
        CoffeeMachine cm = clone.GetComponent<CoffeeMachine>();
        if (cm == null) cm = clone.AddComponent<CoffeeMachine>();

        // Ensure collider exists
        if (clone.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = clone.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = clone.GetComponent<SpriteRenderer>()
                             ?? clone.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                col.size = sr.sprite.bounds.size;
        }
    }
}
