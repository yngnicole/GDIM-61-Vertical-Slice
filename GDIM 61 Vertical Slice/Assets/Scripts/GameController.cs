using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject _speechBubblePrefab;

    public GameObject CoffeeIconPrefab => _speechBubblePrefab;
    public static GameController Instance { get; private set; }

    void Awake() => Instance = this;
    void OnEnable() => GameEvents.OnNPCSpawned += HandleNPCSpawned;
    void OnDisable() => GameEvents.OnNPCSpawned -= HandleNPCSpawned;

    void HandleNPCSpawned(NPC npc)
    {
        // Only let an NPC order a color the cafe can actually brew.
        bool hasBlue = false, hasRed = false;
        foreach (CoffeeMachine cm in Object.FindObjectsOfType<CoffeeMachine>())
        {
            if (cm.MachineColor == OrderType.Blue) hasBlue = true;
            else hasRed = true;
        }

        OrderType pick;
        if (hasBlue && hasRed) pick = Random.value < 0.5f ? OrderType.Blue : OrderType.Red;
        else if (hasBlue)      pick = OrderType.Blue;
        else                   pick = OrderType.Red;

        npc.AssignOrder(pick);
        npc.OnArrived += () => OnNPCArrived(npc);
    }

    void OnNPCArrived(NPC npc)
    {
        SpawnSpeechBubble(npc);
        npc.BeginWaiting();
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

        OrderBubble orderBubble = bubble.GetComponent<OrderBubble>()
                               ?? bubble.AddComponent<OrderBubble>();
        orderBubble.Init(npc);

        // Bubble is purely visual now — delivery happens by clicking the NPC.
        // Strip any collider so it doesn't swallow clicks meant for the NPC underneath.
        foreach (Collider2D bubbleCol in bubble.GetComponentsInChildren<Collider2D>(true))
            Destroy(bubbleCol);

        if (npc.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D npcCol = npc.gameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer npcSr = npc.GetComponent<SpriteRenderer>()
                                ?? npc.GetComponentInChildren<SpriteRenderer>();
            if (npcSr != null && npcSr.sprite != null)
            {
                npcCol.size = npcSr.sprite.bounds.size;
                npcCol.offset = npcSr.sprite.bounds.center;
            }
        }
    }
}
