using System.Collections;
using UnityEngine;

public class NPCGameController : MonoBehaviour
{
    [SerializeField] GameObject _npcPrefab;
    [SerializeField] Transform _spawnPoint;

    const int MaxNPCs = 5;
    const float MinInterval = 1f;
    const float MaxInterval = 5f;

    static readonly Vector3[] Slots = new Vector3[]
    {
        new Vector3(-5f,  -3.5f, 0f),
        new Vector3(-3.5f, -3.5f, 0f),
        new Vector3(-2f,  -3.5f, 0f),
        new Vector3(-0.5f, -3.5f, 0f),
        new Vector3( 1f,  -3.5f, 0f),
    };

    readonly bool[] _slotOccupied = new bool[MaxNPCs];
    int _activeCount;
    Transform _runtimeSpawn;

    void Start()
    {
        SetupSpawnPoint();
        StartCoroutine(SpawnLoop());
    }

    void SetupSpawnPoint()
    {
        bool isUI = _spawnPoint != null && _spawnPoint.GetComponentInParent<Canvas>() != null;
        if (isUI || _spawnPoint == null)
        {
            GameObject go = new GameObject("NPC Spawn (World)");
            go.transform.position = new Vector3(-8f, -4f, 0f);
            _runtimeSpawn = go.transform;
        }
        else
        {
            _runtimeSpawn = _spawnPoint;
        }
    }

    IEnumerator SpawnLoop()
    {
        // Spawn first NPC immediately
        TrySpawnNPC();

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(MinInterval, MaxInterval));
            TrySpawnNPC();
        }
    }

    void TrySpawnNPC()
    {
        if (_npcPrefab == null || _activeCount >= MaxNPCs) return;

        int slot = GetFreeSlot();
        if (slot < 0) return;

        Vector3 jitter = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.5f, 0.5f), 0f);
        GameObject destGo = new GameObject("NPC Dest " + slot);
        destGo.transform.position = Slots[slot] + jitter;

        GameObject npcObj = Instantiate(_npcPrefab, _runtimeSpawn.position, Quaternion.identity);
        NPC npc = npcObj.GetComponent<NPC>();
        if (npc == null) { Destroy(npcObj); Destroy(destGo); return; }

        npc.SetDestination(destGo.transform);
        npc.SetExitPoint(_runtimeSpawn);
        npc.OnLeft = () => FreeSlot(slot, destGo);

        if (npcObj.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = npcObj.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = npcObj.GetComponent<SpriteRenderer>()
                             ?? npcObj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                col.size = sr.sprite.bounds.size;
        }

        _slotOccupied[slot] = true;
        _activeCount++;

        GameEvents.OnNPCSpawned?.Invoke(npc);
    }

    int GetFreeSlot()
    {
        for (int i = 0; i < _slotOccupied.Length; i++)
            if (!_slotOccupied[i]) return i;
        return -1;
    }

    void FreeSlot(int slot, GameObject destGo)
    {
        _slotOccupied[slot] = false;
        _activeCount = Mathf.Max(0, _activeCount - 1);
        if (destGo != null) Destroy(destGo);
    }
}
