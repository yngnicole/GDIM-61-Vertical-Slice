using System.Collections;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    [SerializeField] float _brewTime = 3f;
    [SerializeField] float _coffeeTTL = 10f;

    SpriteRenderer _spriteRenderer;
    Color _defaultColor;
    bool _isBrewing = false;
    bool _drinkReady = false;
    GameObject _coffeeIcon;
    float _coffeeTimer;
    float _brewElapsed;
    TextMesh _timerLabel;

    public bool IsBrewing => _isBrewing;
    public bool IsDrinkReady => _drinkReady;
    public OrderType MachineColor { get; private set; } = OrderType.Red;
    public int BrewCost => MachineColor == OrderType.Blue ? 5 : 3;

    public void SetMachineColor(OrderType c) { MachineColor = c; UpdateLabel(); }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>()
                       ?? GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null) _defaultColor = _spriteRenderer.color;
        EnsureTimerLabel();
        UpdateLabel();
    }

    public void StartBrewing()
    {
        if (_isBrewing || _drinkReady) return;
        StartCoroutine(BrewCoroutine());
    }

    public void OnPickedUp()
    {
        _drinkReady = false;
        _isBrewing = false;
        if (_spriteRenderer != null) _spriteRenderer.color = _defaultColor;
        if (_coffeeIcon != null) { Destroy(_coffeeIcon); _coffeeIcon = null; }
        UpdateLabel();
    }

    public void ResetToIdle()
    {
        StopAllCoroutines();
        _isBrewing = false;
        _drinkReady = false;
        if (_coffeeIcon != null) { Destroy(_coffeeIcon); _coffeeIcon = null; }

        // Wipe any cloned-in timer label and start fresh; clones bring stale ones along.
        if (_timerLabel != null) { Destroy(_timerLabel.gameObject); _timerLabel = null; }
        foreach (Transform child in transform)
            if (child.name == "Timer") Destroy(child.gameObject);

        _spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
            _defaultColor = Color.white;
        }
        EnsureTimerLabel();
        UpdateLabel();
    }

    void Update()
    {
        if (_drinkReady)
        {
            _coffeeTimer -= Time.deltaTime;
            if (_coffeeTimer <= 0f) { WasteDrink(); return; }
        }
        else if (_isBrewing)
        {
            _brewElapsed += Time.deltaTime;
        }
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (_timerLabel == null) return;
        if (_drinkReady)
        {
            _timerLabel.color = Color.yellow;
            _timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, _coffeeTimer)) + "s";
        }
        else if (_isBrewing)
        {
            _timerLabel.color = Color.yellow;
            float remaining = Mathf.Max(0f, _brewTime - _brewElapsed);
            _timerLabel.text = Mathf.CeilToInt(remaining) + "s";
        }
        else
        {
            _timerLabel.color = Color.white;
            _timerLabel.text = "$" + BrewCost + " (" + Mathf.CeilToInt(_brewTime) + "s)";
        }
    }

    IEnumerator BrewCoroutine()
    {
        _isBrewing = true;
        _brewElapsed = 0f;
        if (_spriteRenderer != null) _spriteRenderer.color = new Color(1f, 0.8f, 0.5f);

        yield return new WaitForSeconds(_brewTime);

        _isBrewing = false;
        _drinkReady = true;
        _coffeeTimer = _coffeeTTL;
        if (_spriteRenderer != null) _spriteRenderer.color = new Color(0.5f, 1f, 0.5f);

        SpawnCoffeeIcon();

        if (OrderManager.Instance != null) OrderManager.Instance.OnBrewingComplete();
    }

    void EnsureTimerLabel()
    {
        if (_timerLabel != null) return;
        var go = new GameObject("Timer");
        go.transform.SetParent(transform, false);

        Vector3 ls = transform.lossyScale;
        float invX = !Mathf.Approximately(ls.x, 0f) ? 1f / ls.x : 1f;
        float invY = !Mathf.Approximately(ls.y, 0f) ? 1f / ls.y : 1f;
        go.transform.localScale = new Vector3(invX, invY, 1f);
        go.transform.localPosition = new Vector3(0f, -0.15f * invY, 0f);

        _timerLabel = go.AddComponent<TextMesh>();
        _timerLabel.characterSize = 0.1f;
        _timerLabel.fontSize = 32;
        _timerLabel.anchor = TextAnchor.UpperCenter;
        _timerLabel.alignment = TextAlignment.Center;
        _timerLabel.color = Color.white;
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 100;
    }

    void WasteDrink() => OnPickedUp();

    void SpawnCoffeeIcon()
    {
        if (GameController.Instance == null || GameController.Instance.CoffeeIconPrefab == null) return;
        _coffeeIcon = Instantiate(GameController.Instance.CoffeeIconPrefab);
        _coffeeIcon.transform.position = transform.position + new Vector3(0, 1.5f, 0);
        var sr = _coffeeIcon.GetComponent<SpriteRenderer>() ?? _coffeeIcon.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = MachineColor == OrderType.Blue
                ? new Color(0.45f, 0.7f, 1f)
                : new Color(1f, 0.5f, 0.5f);
        }
    }
}
