using UnityEngine;
using UnityEngine.UI;

public class PriceTagUpdater : MonoBehaviour
{
    [SerializeField] int _cost;
    [SerializeField] Text _label;

    static readonly Color Affordable = new Color(0.3f, 0.9f, 0.3f);
    static readonly Color TooExpensive = new Color(1f, 0.35f, 0.35f);

    public void Init(int cost, Text label)
    {
        _cost = cost;
        _label = label;
    }

    void OnEnable() => Refresh();
    void Update() => Refresh();

    void Refresh()
    {
        if (_label == null) return;
        int money = OrderManager.Instance != null ? OrderManager.Instance.Money : 0;
        _label.color = money >= _cost ? Affordable : TooExpensive;
    }
}
