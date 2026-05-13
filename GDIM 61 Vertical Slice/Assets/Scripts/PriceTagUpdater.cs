using UnityEngine;
using UnityEngine.UI;

public class PriceTagUpdater : MonoBehaviour
{
    [SerializeField] string _itemId;
    [SerializeField] int _cost;
    [SerializeField] Text _label;

    static readonly Color Affordable = new Color(0.3f, 0.9f, 0.3f);
    static readonly Color Unavailable = new Color(1f, 0.35f, 0.35f);

    public void Init(string itemId, int cost, Text label)
    {
        _itemId = itemId;
        _cost = cost;
        _label = label;
    }

    void OnEnable() => Refresh();
    void Update() => Refresh();

    void Refresh()
    {
        if (_label == null) return;

        if (ShopUI.IsPurchased(_itemId))
        {
            _label.text = "exists";
            _label.color = Unavailable;
            return;
        }

        _label.text = "$" + _cost;
        int money = OrderManager.Instance != null ? OrderManager.Instance.Money : 0;
        _label.color = money >= _cost ? Affordable : Unavailable;
    }
}
