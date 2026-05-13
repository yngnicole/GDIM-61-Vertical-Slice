using System;
using UnityEngine;

public enum OrderType { Blue, Red, Muffin, Cake }

public static class OrderInfo
{
    public static float Patience(OrderType t)
    {
        switch (t)
        {
            case OrderType.Blue:   return 10f;
            case OrderType.Red:    return 8f;
            case OrderType.Muffin: return 12f;
            case OrderType.Cake:   return 12f;
            default:               return 8f;
        }
    }

    public static int SellPrice(OrderType t)
    {
        switch (t)
        {
            case OrderType.Blue:   return 15;
            case OrderType.Red:    return 10;
            case OrderType.Muffin: return 8;
            case OrderType.Cake:   return 15;
            default:               return 10;
        }
    }

    public static Color Tint(OrderType t)
    {
        switch (t)
        {
            case OrderType.Blue:   return new Color(0.45f, 0.7f, 1f);
            case OrderType.Red:    return new Color(1f, 0.5f, 0.5f);
            case OrderType.Muffin: return new Color(0.95f, 0.75f, 0.4f);
            case OrderType.Cake:   return new Color(1f, 0.6f, 0.85f);
            default:               return Color.white;
        }
    }
}

public static class GameEvents
{
    public static Action<NPC> OnNPCSpawned;
}
