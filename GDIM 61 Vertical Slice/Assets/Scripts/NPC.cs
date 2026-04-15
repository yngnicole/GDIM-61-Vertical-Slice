using System;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    Transform _destination;
    bool hasArrived = false;
    bool _leaving = false;
    Transform _exitPoint;

    public event Action OnArrived;

    public void SetDestination(Transform dest)
    {
        _destination = dest;
    }

    public void SetExitPoint(Transform exit)
    {
        _exitPoint = exit;
    }

    void Update()
    {
        if (_leaving)
        {
            MoveToExit();
            return;
        }
        MoveInCafe();
    }

    private void MoveInCafe()
    {
        if (_destination == null || hasArrived) return;

        if (Vector3.Distance(transform.position, _destination.position) > 0.1f)
        {
            Vector3 direction = (_destination.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            hasArrived = true;
            OnArrived?.Invoke();
        }
    }

    private void MoveToExit()
    {
        if (_exitPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 target = _exitPoint.position;
        if (Vector3.Distance(transform.position, target) > 0.1f)
        {
            Vector3 direction = (target - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OrderFulfilled()
    {
        // Destroy speech bubble if still exists
        var bubble = GetComponentInChildren<OrderBubble>();
        if (bubble != null)
            Destroy(bubble.gameObject);

        _leaving = true;
    }
}
