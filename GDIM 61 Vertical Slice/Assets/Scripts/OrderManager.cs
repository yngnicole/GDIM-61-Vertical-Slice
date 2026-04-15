using UnityEngine;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    public enum PlayerState
    {
        Idle,           // Waiting for an NPC order to click
        OrderTaken,     // Clicked NPC bubble, now go click a machine
        Brewing,        // Machine is brewing, wait for it
        DrinkReady,     // Machine done, click to pick up
        HoldingDrink,   // Holding drink, click NPC to deliver
    }

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    [Header("Money")]
    [SerializeField] int _startingMoney = 0;
    [SerializeField] int _moneyPerOrder = 10;

    int _money;
    public int Money => _money;

    Text _moneyText;
    Text _statusText;

    // The NPC whose order we're currently working on
    NPC _activeNPC;
    public NPC ActiveNPC => _activeNPC;

    // The machine currently brewing
    CoffeeMachine _activeMachine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _money = _startingMoney;
    }

    void Update()
    {
        UpdateStatusUI();

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[Order] No main camera found!");
            return;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = new Vector2(worldPos.x, worldPos.y);

        Debug.Log("[Order] Click at world pos: " + point + " | Current state: " + CurrentState);

        // Get all colliders at the click point
        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        if (hits.Length == 0)
        {
            Debug.Log("[Order] No colliders hit at click point");
            return;
        }

        Debug.Log("[Order] Hit " + hits.Length + " collider(s): "
            + string.Join(", ", System.Array.ConvertAll(hits, h => h.gameObject.name)));

        foreach (Collider2D hit in hits)
        {
            GameObject clicked = hit.gameObject;

            // Check for OrderBubble (take order)
            OrderBubble bubble = clicked.GetComponent<OrderBubble>();
            if (bubble == null) bubble = clicked.GetComponentInParent<OrderBubble>();
            if (bubble != null && bubble.Owner != null && CurrentState == PlayerState.Idle)
            {
                TakeOrder(bubble.Owner);
                bubble.OnOrderTaken();
                Debug.Log("[Order] Took order from NPC");
                return;
            }

            // Check for CoffeeMachine
            CoffeeMachine machine = clicked.GetComponent<CoffeeMachine>();
            if (machine == null) machine = clicked.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                if (CurrentState == PlayerState.OrderTaken && !machine.IsBrewing)
                {
                    StartBrewing(machine);
                    Debug.Log("[Order] Started brewing on machine");
                    return;
                }
                else if (CurrentState == PlayerState.DrinkReady && machine.IsDrinkReady)
                {
                    PickUpDrink();
                    machine.OnPickedUp();
                    Debug.Log("[Order] Picked up drink from machine");
                    return;
                }
            }

            // Check for NPC (deliver order)
            NPC npc = clicked.GetComponent<NPC>();
            if (npc == null) npc = clicked.GetComponentInParent<NPC>();
            if (npc != null && CurrentState == PlayerState.HoldingDrink && npc == _activeNPC)
            {
                FulfillOrder(npc);
                Debug.Log("[Order] Delivered order! Money: $" + _money);
                return;
            }
        }
    }

    public void TakeOrder(NPC npc)
    {
        if (CurrentState != PlayerState.Idle) return;
        _activeNPC = npc;
        CurrentState = PlayerState.OrderTaken;
    }

    public void StartBrewing(CoffeeMachine machine)
    {
        if (CurrentState != PlayerState.OrderTaken) return;
        _activeMachine = machine;
        CurrentState = PlayerState.Brewing;
        machine.StartBrewing();
    }

    public void OnBrewingComplete()
    {
        if (CurrentState != PlayerState.Brewing) return;
        CurrentState = PlayerState.DrinkReady;
        Debug.Log("[Order] Brewing complete! Click machine to pick up.");
    }

    public void PickUpDrink()
    {
        if (CurrentState != PlayerState.DrinkReady) return;
        _activeMachine = null;
        CurrentState = PlayerState.HoldingDrink;
    }

    public void FulfillOrder(NPC npc)
    {
        if (CurrentState != PlayerState.HoldingDrink) return;
        if (npc != _activeNPC) return;

        _money += _moneyPerOrder;
        UpdateMoneyUI();

        _activeNPC.OrderFulfilled();
        _activeNPC = null;
        CurrentState = PlayerState.Idle;
    }

    public void SetMoneyText(Text text)
    {
        _moneyText = text;
        UpdateMoneyUI();
    }

    public void SetStatusText(Text text)
    {
        _statusText = text;
    }

    void UpdateMoneyUI()
    {
        if (_moneyText != null)
            _moneyText.text = "$" + _money;
    }

    void UpdateStatusUI()
    {
        if (_statusText == null) return;

        switch (CurrentState)
        {
            case PlayerState.Idle:
                _statusText.text = "Click the order icon above NPC";
                break;
            case PlayerState.OrderTaken:
                _statusText.text = "Click a coffee machine to brew";
                break;
            case PlayerState.Brewing:
                _statusText.text = "Brewing... please wait";
                break;
            case PlayerState.DrinkReady:
                _statusText.text = "Click the green machine to pick up";
                break;
            case PlayerState.HoldingDrink:
                _statusText.text = "Click the NPC to deliver";
                break;
        }
    }
}
