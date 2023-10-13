using System;

public static class StaticEventHandler
{
    public static event Action<RoomChangedEventArgs> OnRoomChanged;
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;
    public static event Action<PointScoredArgs> OnPointScored;
    public static event Action<ScoreChangedArgs> OnScoreChanged;
    public static event Action<MultiplierArgs> OnMultiplier;
    public static event Action<BossHealthUIArgs> OnBossHealthUI;
    public static event Action<BossHealthChangeArgs> OnBossHealthChange;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }

    public static void CallRoomEnemiesDefeatedEvent(Room room)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs() { room = room });
    }

    public static void CallPointScoredEvent(int points)
    {
        OnPointScored?.Invoke(new PointScoredArgs() { points = points });
    }

    public static void CallScoreChangedEvent(long score, int multiplier)
    {
        OnScoreChanged?.Invoke(new ScoreChangedArgs() { score = score, multiplier = multiplier });
    }

    public static void CallMultiplierEvent(bool multiplier)
    {
        OnMultiplier?.Invoke(new MultiplierArgs() { multiplier = multiplier });
    }

    public static void CallBossHealthUIEvent(bool showBossHealthBar)
    {
        OnBossHealthUI?.Invoke(new BossHealthUIArgs() { showBossHealthBar = showBossHealthBar });
    }

    public static void CallBossHealthChangeEvent(float healthPercent)
    {
        OnBossHealthChange?.Invoke(new BossHealthChangeArgs() { healthPercent = healthPercent });
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}

public class PointScoredArgs : EventArgs
{
    public int points;
}

public class ScoreChangedArgs : EventArgs
{
    public long score;
    public int multiplier;
}

public class MultiplierArgs : EventArgs
{
    public bool multiplier;
}

public class BossHealthUIArgs : EventArgs
{
    public bool showBossHealthBar;    
}

public class BossHealthChangeArgs : EventArgs
{
    public float healthPercent;
}
