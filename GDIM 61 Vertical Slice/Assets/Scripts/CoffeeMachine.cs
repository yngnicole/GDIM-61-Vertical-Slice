using System.Collections;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    [SerializeField] float _brewTime = 3f;

    SpriteRenderer _spriteRenderer;
    Color _defaultColor;
    bool _isBrewing = false;
    bool _drinkReady = false;
    GameObject _coffeeIcon;

    public bool IsBrewing => _isBrewing;
    public bool IsDrinkReady => _drinkReady;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _defaultColor = _spriteRenderer.color;
    }

    public void StartBrewing()
    {
        StartCoroutine(BrewCoroutine());
    }

    public void OnPickedUp()
    {
        _drinkReady = false;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _defaultColor;

        if (_coffeeIcon != null)
        {
            Destroy(_coffeeIcon);
            _coffeeIcon = null;
        }
    }

    public void ResetToIdle()
    {
        _isBrewing = false;
        _drinkReady = false;
        if (_coffeeIcon != null) { Destroy(_coffeeIcon); _coffeeIcon = null; }
        _spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
            _defaultColor = Color.white;
        }
    }

    IEnumerator BrewCoroutine()
    {
        _isBrewing = true;
        if (_spriteRenderer != null)
            _spriteRenderer.color = new Color(1f, 0.8f, 0.5f);

        yield return new WaitForSeconds(_brewTime);

        _isBrewing = false;
        _drinkReady = true;
        if (_spriteRenderer != null)
            _spriteRenderer.color = new Color(0.5f, 1f, 0.5f);

        // Spawn coffee icon above machine
        SpawnCoffeeIcon();

        if (OrderManager.Instance != null)
            OrderManager.Instance.OnBrewingComplete();
    }

    void SpawnCoffeeIcon()
    {
        if (GameController.Instance != null && GameController.Instance.CoffeeIconPrefab != null)
        {
            _coffeeIcon = Instantiate(GameController.Instance.CoffeeIconPrefab);
            _coffeeIcon.transform.position = transform.position + new Vector3(0, 1.5f, 0);
        }
    }
}
