using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] int _startingMoney = 50;

    const int BlueBrewCost = 5;
    const int RedBrewCost = 3;
    const int BlueSellPrice = 15;
    const int RedSellPrice = 10;
    const int AbandonPenalty = 1;

    enum HandState { Empty, HoldingBlue, HoldingRed }
    HandState _hand = HandState.Empty;

    int _money;
    public int Money => _money;
    public bool HoldingDrink => _hand != HandState.Empty;

    Text _moneyText;
    int _lastDelta;
    float _deltaShowUntil;
    const float DeltaDisplaySeconds = 1.5f;

    GameObject _handIcon;
    SpriteRenderer _handIconSr;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _money = _startingMoney;
    }

    void Update()
    {
        // Re-render every frame so the delta clears itself once the window expires.
        UpdateMoneyUI();
        UpdateHandIcon();
        if (Input.GetMouseButtonDown(0)) HandleClick();
    }

    void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[OrderManager] click swallowed by UI");
            return;
        }

        var cam = UnityEngine.Camera.main;
        if (cam == null) { Debug.LogWarning("[OrderManager] no Camera.main"); return; }

        Vector2 point = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        Debug.Log("[OrderManager] click at " + point + " hand=" + _hand + " hits=" + hits.Length);
        if (hits.Length == 0) return;

        foreach (Collider2D hit in hits)
        {
            GameObject obj = hit.gameObject;
            Debug.Log("[OrderManager]   hit: '" + obj.name + "' parent='" + (obj.transform.parent != null ? obj.transform.parent.name : "(none)") + "'"
                + " hasNPC=" + (obj.GetComponent<NPC>() != null) + " hasNPCInParent=" + (obj.GetComponentInParent<NPC>() != null)
                + " hasMachine=" + (obj.GetComponent<CoffeeMachine>() != null) + " hasMachineInParent=" + (obj.GetComponentInParent<CoffeeMachine>() != null)
                + " hasBubble=" + (obj.GetComponent<OrderBubble>() != null) + " hasBubbleInParent=" + (obj.GetComponentInParent<OrderBubble>() != null));

            // Bubble has no collider, but be defensive in case one is added later —
            // a bubble click should never deliver.
            if (obj.GetComponent<OrderBubble>() != null
                || obj.GetComponentInParent<OrderBubble>() != null) continue;

            CoffeeMachine machine = obj.GetComponent<CoffeeMachine>()
                                 ?? obj.GetComponentInParent<CoffeeMachine>();
            if (machine != null)
            {
                if (HandleMachineClick(machine)) return;
                continue;
            }

            NPC npc = obj.GetComponent<NPC>() ?? obj.GetComponentInParent<NPC>();
            if (npc != null && TryDeliver(npc)) return;
        }
    }

    bool HandleMachineClick(CoffeeMachine machine)
    {
        // Pick up: only when hand is empty and a drink is ready.
        if (_hand == HandState.Empty && machine.IsDrinkReady)
        {
            _hand = machine.MachineColor == OrderType.Blue
                ? HandState.HoldingBlue
                : HandState.HoldingRed;
            machine.OnPickedUp();
            return true;
        }

        // Brew: allowed even while holding a drink — keeps machines busy in parallel.
        if (!machine.IsBrewing && !machine.IsDrinkReady)
        {
            int cost = machine.MachineColor == OrderType.Blue ? BlueBrewCost : RedBrewCost;
            if (TrySpendMoney(cost)) machine.StartBrewing();
            return true;
        }

        return false;
    }

    bool TryDeliver(NPC npc)
    {
        if (_hand == HandState.Empty)
        {
            Debug.Log("[OrderManager] click NPC '" + npc.gameObject.name + "' but hand is empty");
            return false;
        }
        if (!npc.OrderActive)
        {
            Debug.Log("[OrderManager] click NPC '" + npc.gameObject.name + "' but order not active");
            return false;
        }

        bool match = (_hand == HandState.HoldingBlue && npc.OrderType == OrderType.Blue)
                  || (_hand == HandState.HoldingRed && npc.OrderType == OrderType.Red);
        if (!match)
        {
            Debug.Log("[OrderManager] color mismatch — hand=" + _hand + " order=" + npc.OrderType);
            return false;
        }

        int reward = npc.OrderType == OrderType.Blue ? BlueSellPrice : RedSellPrice;
        _money += reward;
        ShowDelta(reward);
        _hand = HandState.Empty;
        npc.OrderFulfilled();
        return true;
    }

    public bool TrySpendMoney(int amount)
    {
        if (_money < amount) return false;
        _money -= amount;
        ShowDelta(-amount);
        return true;
    }

    public void OnNPCAbandoned()
    {
        _money -= AbandonPenalty;
        ShowDelta(-AbandonPenalty);
    }

    void ShowDelta(int amount)
    {
        if (amount == 0) return;
        _lastDelta = amount;
        _deltaShowUntil = Time.time + DeltaDisplaySeconds;
    }

    public void OnBrewingComplete() { }

    void UpdateHandIcon()
    {
        if (_hand == HandState.Empty)
        {
            if (_handIcon != null) { Destroy(_handIcon); _handIcon = null; _handIconSr = null; }
            return;
        }

        if (_handIcon == null) CreateHandIcon();
        if (_handIcon == null) return;

        if (_handIconSr != null)
        {
            _handIconSr.color = _hand == HandState.HoldingBlue
                ? new Color(0.45f, 0.7f, 1f)
                : new Color(1f, 0.5f, 0.5f);
        }

        var cam = UnityEngine.Camera.main;
        if (cam != null)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            _handIcon.transform.position = worldPos;
        }
    }

    void CreateHandIcon()
    {
        if (GameController.Instance == null || GameController.Instance.CoffeeIconPrefab == null) return;
        _handIcon = Instantiate(GameController.Instance.CoffeeIconPrefab);
        _handIcon.name = "HandCoffeeIcon";
        // Strip colliders so the icon doesn't catch clicks meant for the world.
        foreach (var c in _handIcon.GetComponentsInChildren<Collider2D>(true)) Destroy(c);
        _handIconSr = _handIcon.GetComponent<SpriteRenderer>()
                   ?? _handIcon.GetComponentInChildren<SpriteRenderer>();
        if (_handIconSr != null) _handIconSr.sortingOrder = 200;
        _handIcon.transform.localScale = Vector3.one * 0.5f;
    }

    public void SetMoneyText(Text text)
    {
        _moneyText = text;
        if (_moneyText != null) _moneyText.supportRichText = true;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (_moneyText == null) return;

        bool showDelta = Time.time < _deltaShowUntil && _lastDelta != 0;
        string moneyPart = "<color=#FFFF00>Money: $" + _money + "</color>";

        if (showDelta)
        {
            string color = _lastDelta > 0 ? "#33FF33" : "#FF6666";
            string sign = _lastDelta > 0 ? "+$" : "-$";
            string deltaPart = "<color=" + color + ">" + sign + Mathf.Abs(_lastDelta) + "</color>";
            _moneyText.text = deltaPart + " (" + moneyPart + ")";
        }
        else
        {
            _moneyText.text = moneyPart;
        }
    }
}
