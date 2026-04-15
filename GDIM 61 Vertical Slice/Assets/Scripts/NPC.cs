using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NPC : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    Transform _destination;
    bool hasArrived = false;

    public event Action OnArrived;
    public void SetDestination(Transform dest)
    {
        _destination = dest;
    }
    void Update()
    {
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



        //movement to walk out the cafe and disappears. 
        //order icon that pops up and disappears once complete. 
    }
}
