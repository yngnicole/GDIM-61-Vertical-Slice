using UnityEngine;

public class OrderBubble : MonoBehaviour
{
    const float BubbleExtraScale = 1.3f;

    NPC _owner;
    SpriteRenderer _bubbleSr;
    GameObject _iconGo;

    public NPC Owner => _owner;

    public void Init(NPC owner) => _owner = owner;

    void Start()
    {
        // The speech prefab nests an aseprite import. Multi-layer aseprites
        // create multiple SpriteRenderer children — keep the first one (skin
        // it as the white bubble), disable the rest so they don't show as
        // ghost icons.
        SpriteRenderer[] allSrs = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < allSrs.Length; i++)
        {
            if (i == 0)
            {
                _bubbleSr = allSrs[i];
                Sprite white = Resources.Load<Sprite>("Bubbles/white_bubble");
                if (white != null) _bubbleSr.sprite = white;
                _bubbleSr.enabled = true;
                _bubbleSr.color = BubbleTint(_owner != null ? _owner.OrderType : OrderType.Red);
                // Make the bubble background larger without scaling the icon
                // (icon counter-scales below).
                _bubbleSr.transform.localScale *= BubbleExtraScale;
                // Nudge the bubble down by 5% of its visible height.
                float dy = _bubbleSr.bounds.size.y * 0.05f;
                _bubbleSr.transform.position += new Vector3(0f, -dy, 0f);
            }
            else
            {
                allSrs[i].enabled = false;
            }
        }

        SpawnIcon();
    }

    static Color BubbleTint(OrderType t)
    {
        switch (t)
        {
            case OrderType.Blue:   return new Color(0.45f, 0.7f, 1f);   // blue
            case OrderType.Red:    return new Color(1f, 0.5f, 0.5f);    // red/pink
            case OrderType.Muffin:
            case OrderType.Cake:   return new Color(1f, 0.85f, 0.4f);   // yellow (both)
            default:               return Color.white;
        }
    }

    void SpawnIcon()
    {
        if (_owner == null) return;
        string iconName = IconNameFor(_owner.OrderType);
        if (string.IsNullOrEmpty(iconName)) return;

        Sprite iconSprite = Resources.Load<Sprite>("Bubbles/" + iconName);
        if (iconSprite == null)
        {
            Debug.LogWarning("[OrderBubble] Missing icon sprite at Resources/Bubbles/" + iconName);
            return;
        }

        _iconGo = new GameObject("Icon");
        // Parent under the bubble SR's transform so the icon sits exactly
        // where the bubble is shown (the prefab applies an offset/scale on
        // that nested transform).
        Transform parent = _bubbleSr != null ? _bubbleSr.transform : transform;
        _iconGo.transform.SetParent(parent, false);
        _iconGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        // Counter-scale the bubble's growth so the icon keeps its prior size.
        _iconGo.transform.localScale = Vector3.one * (0.75f / BubbleExtraScale);

        SpriteRenderer iconSr = _iconGo.AddComponent<SpriteRenderer>();
        iconSr.sprite = iconSprite;
        iconSr.sortingOrder = (_bubbleSr != null ? _bubbleSr.sortingOrder : 11) + 1;
    }

    static string IconNameFor(OrderType t)
    {
        switch (t)
        {
            case OrderType.Blue:   return "blue_coffee_icon";
            case OrderType.Red:    return "red_coffee_icon";
            case OrderType.Muffin: return "muffin_icon";
            case OrderType.Cake:   return "cake_icon";
            default:               return null;
        }
    }

    void Update()
    {
        if (_owner != null)
            transform.position = _owner.transform.position + new Vector3(0, 1.5f, 0);
    }
}
