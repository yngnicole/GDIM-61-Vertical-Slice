using System;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] float speed = 3f;

    Transform _destination;
    Transform _exitPoint;
    bool _hasArrived = false;
    bool _leaving = false;

    OrderType _orderType;
    float _patienceLeft;
    bool _orderActive = false;
    bool _wasFulfilled = false;
    TextMesh _timerLabel;

    public Action OnLeft;
    public event Action OnArrived;

    public OrderType OrderType => _orderType;
    public bool OrderActive => _orderActive && !_leaving;
    public bool WasFulfilled => _wasFulfilled;

    public void SetDestination(Transform dest) => _destination = dest;
    public void SetExitPoint(Transform exit) => _exitPoint = exit;

    public void AssignOrder(OrderType type)
    {
        _orderType = type;
        _patienceLeft = type == OrderType.Blue ? 10f : 8f;
    }

    public void BeginWaiting()
    {
        _orderActive = true;
        EnsureTimerLabel();
        Debug.Log("[NPC] " + gameObject.name + " arrived, OrderActive=true, OrderType=" + _orderType
            + ", at " + transform.position);
    }

    void Update()
    {
        if (_leaving) { MoveToExit(); return; }
        MoveInCafe();

        if (_orderActive)
        {
            _patienceLeft -= Time.deltaTime;
            if (_timerLabel != null)
            {
                int price = _orderType == OrderType.Blue ? 15 : 10;
                _timerLabel.text = Mathf.CeilToInt(Mathf.Max(0f, _patienceLeft)) + "s($" + price + ")";
            }
            if (_patienceLeft <= 0f) GiveUp();
        }
    }

    void MoveInCafe()
    {
        if (_destination == null || _hasArrived) return;
        if (Vector3.Distance(transform.position, _destination.position) > 0.1f)
        {
            Vector3 dir = (_destination.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            _hasArrived = true;
            OnArrived?.Invoke();
        }
    }

    void MoveToExit()
    {
        if (_exitPoint == null) { Destroy(gameObject); return; }
        Vector3 target = _exitPoint.position;
        if (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else Destroy(gameObject);
    }

    void GiveUp()
    {
        _orderActive = false;
        var bubble = GetComponentInChildren<OrderBubble>();
        if (bubble != null) Destroy(bubble.gameObject);
        if (_timerLabel != null) { Destroy(_timerLabel.gameObject); _timerLabel = null; }
        if (OrderManager.Instance != null) OrderManager.Instance.OnNPCAbandoned();
        _leaving = true;
    }

    public void OrderFulfilled()
    {
        _wasFulfilled = true;
        _orderActive = false;
        var bubble = GetComponentInChildren<OrderBubble>();
        if (bubble != null) Destroy(bubble.gameObject);
        if (_timerLabel != null) { Destroy(_timerLabel.gameObject); _timerLabel = null; }
        _leaving = true;
    }

    void EnsureTimerLabel()
    {
        if (_timerLabel != null) return;
        var go = new GameObject("Timer");
        go.transform.SetParent(transform, false);

        // Cancel out the parent's scale so the label renders at world scale 1.
        Vector3 ls = transform.lossyScale;
        float invX = !Mathf.Approximately(ls.x, 0f) ? 1f / ls.x : 1f;
        float invY = !Mathf.Approximately(ls.y, 0f) ? 1f / ls.y : 1f;
        go.transform.localScale = new Vector3(invX, invY, 1f);

        // World offset of ~0.15 below the NPC's pivot (bottom of sprite).
        go.transform.localPosition = new Vector3(0f, -0.15f * invY, 0f);

        _timerLabel = go.AddComponent<TextMesh>();
        _timerLabel.characterSize = 0.1f;
        _timerLabel.fontSize = 32;
        _timerLabel.anchor = TextAnchor.UpperCenter;
        _timerLabel.alignment = TextAlignment.Center;
        _timerLabel.color = _orderType == OrderType.Blue
            ? new Color(0.4f, 0.7f, 1f)
            : new Color(1f, 0.5f, 0.5f);
        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 100;
    }

    void OnDestroy() => OnLeft?.Invoke();
}
