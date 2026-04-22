using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] int _startingMoney = 50;
    [SerializeField] int _moneyPerOrder = 10;

    int _money;
    public int Money => _money;

    // All NPCs whose order has been taken but not yet delivered
    readonly HashSet<NPC> _takenOrders = new HashSet<NPC>();

    // Whether the player is currently carrying a drink
    bool _holdingDrink;
    public bool HoldingDrink => _holdingDrink;

    public bool HasTakenOrder(NPC npc) => _takenOrders.Contains(npc);
    public int PendingOrderCount => _takenOrders.Count;

    // First pending NPC (used by AutoTester)
    public NPC ActiveNPC
    {
        get { foreach (NPC n in _takenOrders) return n; return null; }
    }

    Text _moneyText;
    Text _statusText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _money = _startingMoney;
    }

    void Update()
    {
        UpdateStatusUI();
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null) return;

        Vector2 point = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        if (hits.Length == 0) return;

        foreach (Collider2D hit in hits)
        {
            GameObject clicked = hit.gameObject;

            // Take order from an NPC bubble (can take from multiple NPCs)
            OrderBubble bubble = clicked.GetComponent<OrderBubble>()
                              ?? clicked.GetComponentInParent<OrderBubble>();
            if (bubble != null && bubble.Owner != null && !_takenOrders.Contains(bubble.Owner))
            {
                TakeOrder(bubble.Owner);
                bubble.OnOrderTaken();
                return;
            }

            // Coffee machine interactions
            CoffeeMachine machine = clicked.GetComponent<CoffeeMachine>()
                                 ?? clicked.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                // Start brewing if machine is idle and we have pending orders
                if (!machine.IsBrewing && !machine.IsDrinkReady && _takenOrders.Count > 0)
                {
                    StartBrewing(machine);
                    return;
                }
                // Pick up if machine is done and player isn't already holding
                if (machine.IsDrinkReady && !_holdingDrink)
                {
                    PickUpDrink();
                    machine.OnPickedUp();
                    return;
                }
            }

            // Deliver to any NPC whose order was taken
            NPC npc = clicked.GetComponent<NPC>() ?? clicked.GetComponentInParent<NPC>();
            if (npc != null && _holdingDrink && _takenOrders.Contains(npc))
            {
                FulfillOrder(npc);
                return;
            }
        }
    }

    public void TakeOrder(NPC npc)
    {
        _takenOrders.Add(npc);
    }

    public void StartBrewing(CoffeeMachine machine)
    {
        if (machine == null || machine.IsBrewing || machine.IsDrinkReady) return;
        machine.StartBrewing();
    }

    public void PickUpDrink()
    {
        _holdingDrink = true;
    }

    public void FulfillOrder(NPC npc)
    {
        if (!_takenOrders.Contains(npc) || !_holdingDrink) return;
        _takenOrders.Remove(npc);
        _holdingDrink = false;
        _money += _moneyPerOrder;
        UpdateMoneyUI();
        npc.OrderFulfilled();
    }

    // Called by CoffeeMachine when brewing completes — no global state change needed
    public void OnBrewingComplete() { }

    public bool TrySpendMoney(int amount)
    {
        if (_money < amount) return false;
        _money -= amount;
        UpdateMoneyUI();
        return true;
    }

    public void SetMoneyText(Text text) { _moneyText = text; UpdateMoneyUI(); }
    public void SetStatusText(Text text) { _statusText = text; }

    void UpdateMoneyUI()
    {
        if (_moneyText != null) _moneyText.text = "$" + _money;
    }

    void UpdateStatusUI()
    {
        if (_statusText == null) return;

        if (_holdingDrink)
            _statusText.text = "Click an NPC to deliver";
        else if (_takenOrders.Count > 0)
            _statusText.text = "Click a machine to brew";
        else
            _statusText.text = "Click the order icon above NPC";
    }
}
