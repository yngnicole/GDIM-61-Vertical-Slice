using System.Collections;
using System.Collections.Generic;
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
    GameObject[] _prefabPool;

    void Start()
    {
        SetupSpawnPoint();
        BuildPrefabPool();
        StartCoroutine(SpawnLoop());
    }

    void BuildPrefabPool()
    {
        // Pick up every NPC prefab dropped under Assets/Resources/NPCs.
        var loaded = Resources.LoadAll<GameObject>("NPCs");
        var list = new List<GameObject>();
        if (loaded != null) list.AddRange(loaded);
        if (_npcPrefab != null && !list.Contains(_npcPrefab)) list.Add(_npcPrefab);
        _prefabPool = list.ToArray();
        Debug.Log("[NPCGameController] Loaded " + _prefabPool.Length + " NPC prefabs");
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
        TrySpawnNPC();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(MinInterval, MaxInterval));
            TrySpawnNPC();
        }
    }

    void TrySpawnNPC()
    {
        if (_prefabPool == null || _prefabPool.Length == 0 || _activeCount >= MaxNPCs) return;

        int slot = GetFreeSlot();
        if (slot < 0) return;

        GameObject prefab = _prefabPool[Random.Range(0, _prefabPool.Length)];

        Vector3 jitter = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.5f, 0.5f), 0f);
        GameObject destGo = new GameObject("NPC Dest " + slot);
        destGo.transform.position = Slots[slot] + jitter;

        GameObject npcObj = Instantiate(prefab, _runtimeSpawn.position, Quaternion.identity);
        NPC npc = npcObj.GetComponent<NPC>() ?? npcObj.AddComponent<NPC>();

        npc.SetDestination(destGo.transform);
        npc.SetExitPoint(_runtimeSpawn);
        npc.OnLeft = () => FreeSlot(slot, destGo);

        // Add the collider on the SpriteRenderer's GameObject so its local space
        // matches the sprite (for multi-layer aseprite imports the SR sits on a child,
        // not the root). Click handler resolves the NPC via GetComponentInParent.
        SpriteRenderer npcSr = npcObj.GetComponent<SpriteRenderer>()
                            ?? npcObj.GetComponentInChildren<SpriteRenderer>();
        if (npcSr != null && npcSr.sprite != null && npcSr.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = npcSr.gameObject.AddComponent<BoxCollider2D>();
            col.size = npcSr.sprite.bounds.size;
            col.offset = npcSr.sprite.bounds.center;

            string srLoc = (npcSr.transform == npcObj.transform) ? "root" : ("child:" + npcSr.gameObject.name);
            Debug.Log("[NPCGameController] Spawned " + npcObj.name + " (" + prefab.name + ") at " + npcObj.transform.position
                + " — SR on " + srLoc + ", lossyScale=" + npcSr.transform.lossyScale
                + ", sprite.bounds.size=" + npcSr.sprite.bounds.size
                + ", sprite.bounds.center=" + npcSr.sprite.bounds.center);
        }
        else
        {
            Debug.LogWarning("[NPCGameController] Spawned " + npcObj.name + " but did NOT add collider"
                + " (sr=" + (npcSr != null) + ", sprite=" + (npcSr != null && npcSr.sprite != null)
                + ", existingCollider=" + (npcSr != null && npcSr.GetComponent<Collider2D>() != null) + ")");
        }

        // Force NPC visuals above the room background. NPC 2.prefab has no
        // sortingOrder override and would otherwise render at 0 (below the room).
        foreach (var sg in npcObj.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true))
            sg.sortingOrder = Mathf.Max(sg.sortingOrder, 10);
        foreach (var renderer in npcObj.GetComponentsInChildren<SpriteRenderer>(true))
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, 10);

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
