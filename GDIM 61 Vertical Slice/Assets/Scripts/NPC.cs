using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NPC : MonoBehaviour
{
    [SerializeField] Transform _npcTransform;
    [SerializeField] Transform _destination;
    [SerializeField] public float speed = 3f;
    void Update()
    {
        Vector3 direction = (_destination.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;


        // use translate to make npc go into the cafe and then stop
        // so startdestination to stopdestination. 
        //movement into the cafe
        //movement to walk out the cafe and disappears. 
        //order icon that pops up and disappears once complete. 
    }
}
