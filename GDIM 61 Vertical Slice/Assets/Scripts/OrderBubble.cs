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
        _spriteRenderer.color = _owner.OrderType == OrderType.Blue
            ? new Color(0.45f, 0.7f, 1f)
            : new Color(1f, 0.5f, 0.5f);
    }

    void Update()
    {
        if (_owner != null)
            transform.position = _owner.transform.position + new Vector3(0, 1.5f, 0);
    }
}
