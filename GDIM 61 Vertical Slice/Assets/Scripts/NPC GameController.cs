using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGameController : MonoBehaviour
{
    [SerializeField] GameObject _npcPrefab;


    private void Start()
    {
        SpawnNPC();
    }
    public void SpawnNPC()
    {
        Vector3 spawnPosition = new Vector3(-2.86f, -4.34f, 0f);

        Instantiate(_npcPrefab, spawnPosition, UnityEngine.Quaternion.identity);
    }
}
