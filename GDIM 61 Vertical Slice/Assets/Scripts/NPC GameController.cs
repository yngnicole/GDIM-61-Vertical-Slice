using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGameController : MonoBehaviour
{
    [SerializeField] GameObject _npcPrefab;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] Transform _destination;

    private void Start()
    {
    }
    public void SpawnNPC()
    {
        GameObject npcObj = Instantiate(_npcPrefab, _spawnPoint.position, Quaternion.identity);

        NPC npc = npcObj.GetComponent<NPC>();
        npc.SetDestination(_destination);

    }
}

   