using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject _speechBubblePrefab;

    // Expose the coffee icon prefab so CoffeeMachine can spawn one when done
    public GameObject CoffeeIconPrefab => _speechBubblePrefab;
    public static GameController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameEvents.OnNPCSpawned += HandleNPCSpawned;
    }

    void OnDisable()
    {
        GameEvents.OnNPCSpawned -= HandleNPCSpawned;
    }

    void HandleNPCSpawned(NPC npc)
    {
        npc.OnArrived += () => OnNPCArrived(npc);
    }

    void OnNPCArrived(NPC npc)
    {
        SpawnSpeechBubble(npc);
    }

    public void SpawnSpeechBubble(NPC npc)
    {
        if (_speechBubblePrefab == null)
        {
            Debug.LogWarning("[GameController] Speech bubble prefab not assigned!");
            return;
        }

        GameObject bubble = Instantiate(_speechBubblePrefab);
        bubble.transform.position = npc.transform.position + new Vector3(0, 1.5f, 0);
        bubble.transform.SetParent(npc.transform);

        // Add OrderBubble component
        OrderBubble orderBubble = bubble.GetComponent<OrderBubble>();
        if (orderBubble == null)
            orderBubble = bubble.AddComponent<OrderBubble>();
        orderBubble.Init(npc);

        // Ensure collider exists for click detection
        if (bubble.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = bubble.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = bubble.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bubble.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                col.size = sr.sprite.bounds.size;
        }

        // Also ensure the NPC itself has a collider (for delivering the order)
        if (npc.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D npcCol = npc.gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer npcSr = npc.GetComponent<SpriteRenderer>();
            if (npcSr == null) npcSr = npc.GetComponentInChildren<SpriteRenderer>();
            if (npcSr != null && npcSr.sprite != null)
                npcCol.size = npcSr.sprite.bounds.size;
        }

        Debug.Log("[GameController] Spawned order bubble for NPC at " + npc.transform.position);
    }
}
