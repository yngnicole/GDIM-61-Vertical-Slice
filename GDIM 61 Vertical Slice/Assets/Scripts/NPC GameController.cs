using System;
using UnityEngine;

public class NPCGameController : MonoBehaviour
{
    [SerializeField] GameObject _npcPrefab;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] Transform _destination;

    // World-space fallback positions (used if spawn/destination are in Canvas space)
    Vector3 _worldSpawn = new Vector3(-8f, -4f, 0f);
    Vector3 _worldDestination = new Vector3(-3f, -3.5f, 0f);

    Transform _runtimeSpawn;
    Transform _runtimeDestination;

    private void Start()
    {
        SetupWorldPositions();
        SpawnNPC();
    }

    void SetupWorldPositions()
    {
        // Check if the assigned spawn point is inside a Canvas (UI space)
        // If so, create world-space alternatives
        bool spawnIsUI = _spawnPoint != null && _spawnPoint.GetComponentInParent<Canvas>() != null;
        bool destIsUI = _destination != null && _destination.GetComponentInParent<Canvas>() != null;

        if (spawnIsUI || _spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("NPC Spawn (World)");
            spawnObj.transform.position = _worldSpawn;
            _runtimeSpawn = spawnObj.transform;
            Debug.Log("[NPCGameController] Using world-space spawn at " + _worldSpawn);
        }
        else
        {
            _runtimeSpawn = _spawnPoint;
        }

        if (destIsUI || _destination == null)
        {
            GameObject destObj = new GameObject("NPC Destination (World)");
            destObj.transform.position = _worldDestination;
            _runtimeDestination = destObj.transform;
            Debug.Log("[NPCGameController] Using world-space destination at " + _worldDestination);
        }
        else
        {
            _runtimeDestination = _destination;
        }
    }

    public void SpawnNPC()
    {
        if (_npcPrefab == null)
        {
            Debug.LogWarning("[NPCGameController] Missing NPC prefab!");
            return;
        }

        GameObject npcObj = Instantiate(_npcPrefab, _runtimeSpawn.position, Quaternion.identity);

        NPC npc = npcObj.GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogWarning("[NPCGameController] NPC prefab missing NPC component!");
            return;
        }

        npc.SetDestination(_runtimeDestination);
        npc.SetExitPoint(_runtimeSpawn);

        // Ensure NPC has a collider for click detection
        if (npcObj.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = npcObj.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = npcObj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = npcObj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                col.size = sr.sprite.bounds.size;
        }

        Debug.Log("[NPCGameController] Spawned NPC at " + _runtimeSpawn.position
            + " heading to " + _runtimeDestination.position);
        GameEvents.OnNPCSpawned?.Invoke(npc);
    }
}
