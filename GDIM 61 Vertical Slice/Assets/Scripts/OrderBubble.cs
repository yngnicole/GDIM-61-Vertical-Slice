using UnityEngine;

public class OrderBubble : MonoBehaviour
{
    NPC _owner;
    SpriteRenderer _spriteRenderer;

    public NPC Owner => _owner;

    public void Init(NPC owner)
    {
        _owner = owner;
    }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Follow NPC position
        if (_owner != null)
            transform.position = _owner.transform.position + new Vector3(0, 1.5f, 0);
    }

    public void OnOrderTaken()
    {
        // Visual feedback: dim the bubble
        if (_spriteRenderer != null)
            _spriteRenderer.color = new Color(1f, 1f, 0.5f, 0.7f);
    }
}
