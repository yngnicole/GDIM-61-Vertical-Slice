using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] GameObject _speechBubblePrefab;

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
        SpawnSpeechBubble(npc.transform);
    }
    public void SpawnSpeechBubble(Transform npcTransform)
    {
        GameObject bubble = Instantiate(_speechBubblePrefab, npcTransform, false);
        bubble.transform.localPosition = new Vector3(0.2f, 0.25f, -0.1f);
    }
}
