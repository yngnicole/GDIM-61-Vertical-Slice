using UnityEngine;

public class OrderBubble : MonoBehaviour
{
    NPC _owner;
    SpriteRenderer _spriteRenderer;

    public NPC Owner => _owner;

    public void Init(NPC owner) => _owner = owner;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>()
                       ?? GetComponentInChildren<SpriteRenderer>();
        ApplyColor();
    }

    void ApplyColor()
    {
        if (_spriteRenderer == null || _owner == null) return;
        _spriteRenderer.color = OrderInfo.Tint(_owner.OrderType);
    }

    void Update()
    {
        if (_owner != null)
            transform.position = _owner.transform.position + new Vector3(0, 1.5f, 0);
    }
}
