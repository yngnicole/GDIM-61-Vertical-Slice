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

    enum HandState { Empty, HoldingOrder, HoldingCoffee }
    HandState _hand = HandState.Empty;
    NPC _handNPC; // NPC whose order/coffee is in hand

    // Tracks which NPC each machine is brewing for
    readonly Dictionary<CoffeeMachine, NPC> _brewingFor = new Dictionary<CoffeeMachine, NPC>();

    Text _moneyText, _statusText;

    // AutoTester helpers
    public NPC ActiveNPC => _handNPC;
    public bool HoldingDrink => _hand == HandState.HoldingCoffee;

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
            GameObject obj = hit.gameObject;

            // Click NPC bubble while hand is empty → take order
            OrderBubble bubble = obj.GetComponent<OrderBubble>()
                              ?? obj.GetComponentInParent<OrderBubble>();
            if (bubble != null && bubble.Owner != null && _hand == HandState.Empty)
            {
                TakeOrder(bubble.Owner);
                bubble.OnOrderTaken();
                return;
            }

            // Click machine while holding an order → start brewing
            CoffeeMachine machine = obj.GetComponent<CoffeeMachine>()
                                 ?? obj.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                if (_hand == HandState.HoldingOrder
                    && !machine.IsBrewing && !machine.IsDrinkReady)
                {
                    StartBrewing(machine);
                    return;
                }

                // Click done machine while hand is empty → pick up coffee
                if (_hand == HandState.Empty && machine.IsDrinkReady
                    && _brewingFor.TryGetValue(machine, out NPC forNPC))
                {
                    _hand = HandState.HoldingCoffee;
                    _handNPC = forNPC;
                    _brewingFor.Remove(machine);
                    machine.OnPickedUp();
                    return;
                }
            }

            // Click NPC while holding matching coffee → deliver
            NPC npc = obj.GetComponent<NPC>() ?? obj.GetComponentInParent<NPC>();
            if (npc != null && _hand == HandState.HoldingCoffee && npc == _handNPC)
            {
                FulfillOrder(npc);
                return;
            }
        }
    }

    // ── Public API (also used by AutoTester) ────────────────────────────────

    public void TakeOrder(NPC npc)
    {
        if (_hand != HandState.Empty) return;
        _hand = HandState.HoldingOrder;
        _handNPC = npc;
    }

    public void StartBrewing(CoffeeMachine machine)
    {
        if (machine == null || _hand != HandState.HoldingOrder) return;
        if (machine.IsBrewing || machine.IsDrinkReady) return;
        _brewingFor[machine] = _handNPC;
        _hand = HandState.Empty;
        _handNPC = null;
        machine.StartBrewing();
    }

    // Pick up from a specific machine (used by AutoTester)
    public void PickUpFrom(CoffeeMachine machine)
    {
        if (machine == null || !machine.IsDrinkReady) return;
        if (!_brewingFor.TryGetValue(machine, out NPC forNPC)) return;
        _hand = HandState.HoldingCoffee;
        _handNPC = forNPC;
        _brewingFor.Remove(machine);
        machine.OnPickedUp();
    }

    public void FulfillOrder(NPC npc)
    {
        if (_hand != HandState.HoldingCoffee || npc != _handNPC) return;
        _hand = HandState.Empty;
        _handNPC = null;
        _money += _moneyPerOrder;
        UpdateMoneyUI();
        npc.OrderFulfilled();
    }

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
        switch (_hand)
        {
            case HandState.Empty:
                _statusText.text = "Click the order icon above an NPC";
                break;
            case HandState.HoldingOrder:
                _statusText.text = "Click a coffee machine to brew";
                break;
            case HandState.HoldingCoffee:
                _statusText.text = "Click the NPC to deliver";
                break;
        }
    }
}
