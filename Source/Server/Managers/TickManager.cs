using GameServer.Core;
using GameServer.PacketManager;
using RTShared.Files;
using RTShared.Misc;
using static RTShared.Misc.Printer;

namespace GameServer.Managers;

public static class TickManager
{
    public const int TicksPerSecond = 60;

    private static long _lastLoggedTick = 0L;

    public static void StartHeartbeat()
    {
        Printer.Warning("[TimeSync] Heartbeat timer started (interval: " + Master.TickHeartbeatIntervalMs + "ms)", Verbosity.Extreme);

        while (true)
        {
            Thread.Sleep(Master.TickHeartbeatIntervalMs);

            if (Master.GlobalTimePaused) continue;

            AdvanceTick();
            PM_Time.BroadcastTickHeartbeat();
            SaveTimeState();
        }
    }

    public static void AdvanceTick()
    {
        long now = DateTime.UtcNow.Ticks;
        double elapsedSeconds = (now - Master.TickBaseTimestamp) / (double)TimeSpan.TicksPerSecond;
        long tickAdvance = (long)(elapsedSeconds * TicksPerSecond * Master.GlobalTimeSpeed);
        long previousTick = Master.GlobalServerTick;
        Master.GlobalServerTick += tickAdvance;
        Master.TickBaseTimestamp = now;

        if (tickAdvance > 0 && Master.GlobalServerTick - _lastLoggedTick >= TicksPerSecond * 60)
        {
            Printer.Warning($"[TimeSync] Tick={Master.GlobalServerTick} (+{tickAdvance} in {elapsedSeconds:F2}s, speed={Master.GlobalTimeSpeed}x)", Verbosity.Extreme);
            _lastLoggedTick = Master.GlobalServerTick;
        }
    }

    public static void SaveTimeState()
    {
        FL_GameTime state = new FL_GameTime
        {
            TimeSpeed = Master.GlobalTimeSpeed,
            IsPaused = Master.GlobalTimePaused,
            PausedBy = Master.GlobalTimePausedBy,
            ServerTick = Master.GlobalServerTick,
            TickBaseTimestamp = Master.TickBaseTimestamp
        };
        FL_GameTime.Save(FL_GameTime.SavePath, state);
    }
}
