using UnityEngine;

// A clickable in-world display (muffin, cake). Always ready: click picks it
// up directly with no brewing step.
public class FoodItem : MonoBehaviour
{
    public OrderType FoodType { get; private set; } = OrderType.Muffin;
    public Sprite Sprite { get; private set; }

    public void Configure(OrderType type, Sprite sprite)
    {
        FoodType = type;
        Sprite = sprite;
    }
}
