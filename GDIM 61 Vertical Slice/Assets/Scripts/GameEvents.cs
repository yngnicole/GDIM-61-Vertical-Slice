using System;

public enum OrderType { Blue, Red }

public static class GameEvents
{
    public static Action<NPC> OnNPCSpawned;
}
