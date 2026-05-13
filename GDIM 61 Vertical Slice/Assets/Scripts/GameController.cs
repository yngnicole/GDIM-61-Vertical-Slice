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
        // Only let an NPC order something the cafe can actually serve.
        var available = new System.Collections.Generic.HashSet<OrderType>();
        foreach (CoffeeMachine cm in Object.FindObjectsOfType<CoffeeMachine>())
            available.Add(cm.MachineColor);
        foreach (FoodItem food in Object.FindObjectsOfType<FoodItem>())
            available.Add(food.FoodType);

        OrderType pick = OrderType.Red; // safe fallback — scene always has a red machine
        if (available.Count > 0)
        {
            var arr = new OrderType[available.Count];
            available.CopyTo(arr);
            pick = arr[Random.Range(0, arr.Length)];
        }

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
