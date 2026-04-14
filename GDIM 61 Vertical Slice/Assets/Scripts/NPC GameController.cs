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
        //only spawn in if there is less then 1 npcs present. do later 
        Vector3 spawnPosition = new Vector3(-2.86f, -4.34f, 0f);

        Instantiate(_npcPrefab, spawnPosition, UnityEngine.Quaternion.identity);
    }
}
