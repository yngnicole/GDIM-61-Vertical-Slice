using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NPC : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    Transform _destination;

    public void SetDestination(Transform dest)
    {
        _destination = dest;
    }
    void Update()
    {
        if (_destination == null) return;

        if (Vector3.Distance(transform.position, _destination.position) > 0.1f)
        {
            Vector3 direction = (_destination.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }

        // use translate to make npc go into the cafe and then stop
        // so startdestination to stopdestination. 
        //movement into the cafe
        //movement to walk out the cafe and disappears. 
        //order icon that pops up and disappears once complete. 
    }
}
