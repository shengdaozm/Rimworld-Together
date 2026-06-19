using GameServer.Core;
using GameServer.Hooks.TCPNetwork;
using GameServer.Managers;
using GameServer.Misc;
using RTShared;
using RTShared.Misc;
using RTShared.Files.ServerClient;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using static RTNetwork.Packets.PKT_Time;
using static RTShared.Misc.Printer;
using RTNetwork.Components;

namespace GameServer.PacketManager;

public class PM_Time : PM_Base
{
    [HandlesPacket(PacketHeader.GameTime)]
    public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
    {
        PKT_Time data = Serializer.ConvertBytesToObject<PKT_Time>(bytes);
        FL_Player player = client.GetData<FL_Player>();

        switch (data.StepMode)
        {
            case TimeStepMode.SetSpeed:
                Printer.Warning($"[TimeSync] {player.Username} → SetSpeed {data.Speed}x", Verbosity.Extreme);
                HandleSetSpeed(client, data);
                break;

            case TimeStepMode.Pause:
                Printer.Warning($"[TimeSync] {player.Username} → Pause (by: {data.PausedBy})", Verbosity.Extreme);
                HandlePause(client, data);
                break;

            case TimeStepMode.Resume:
                Printer.Warning($"[TimeSync] {player.Username} → Resume", Verbosity.Extreme);
                HandleResume(client, data);
                break;
        }
    }

    private static void HandleSetSpeed(ServerClient client, PKT_Time data)
    {
        if (!client.GetData<FL_Player>().IsAdmin)
        {
            ResponseShortcutManager.SendIllegalPacket(client, "Tried to change global speed without being admin!");
            return;
        }

        float oldSpeed = Master.GlobalTimeSpeed;
        TickManager.AdvanceTick();
        Master.GlobalTimeSpeed = data.Speed;
        Master.TickBaseTimestamp = DateTime.UtcNow.Ticks;
        TickManager.SaveTimeState();

        Printer.Warning($"[TimeSync] Speed changed: {oldSpeed}x → {data.Speed}x (tick={Master.GlobalServerTick})");

        PKT_Time broadcast = new PKT_Time
        {
            StepMode = TimeStepMode.SetSpeed,
            Speed = data.Speed,
            ServerTick = Master.GlobalServerTick
        };

        ServerNetwork.SendPacketToAllClients(PacketHeader.GameTime, broadcast);
    }

    private static void HandlePause(ServerClient client, PKT_Time data)
    {
        if (!client.GetData<FL_Player>().IsAdmin)
        {
            ResponseShortcutManager.SendIllegalPacket(client, "Tried to pause without being admin!");
            return;
        }

        TickManager.AdvanceTick();
        Master.GlobalTimePaused = true;
        Master.GlobalTimePausedBy = data.PausedBy;
        TickManager.SaveTimeState();

        Printer.Warning($"[TimeSync] Game paused by {data.PausedBy} (tick={Master.GlobalServerTick})");

        PKT_Time broadcast = new PKT_Time
        {
            StepMode = TimeStepMode.Pause,
            PausedBy = data.PausedBy,
            ServerTick = Master.GlobalServerTick
        };

        ServerNetwork.SendPacketToAllClients(PacketHeader.GameTime, broadcast);
    }

    private static void HandleResume(ServerClient client, PKT_Time data)
    {
        if (!client.GetData<FL_Player>().IsAdmin)
        {
            ResponseShortcutManager.SendIllegalPacket(client, "Tried to resume without being admin!");
            return;
        }

        Master.GlobalTimePaused = false;
        Master.GlobalTimePausedBy = null;
        Master.TickBaseTimestamp = DateTime.UtcNow.Ticks;
        TickManager.SaveTimeState();

        Printer.Warning($"[TimeSync] Game resumed (tick={Master.GlobalServerTick})");

        PKT_Time broadcast = new PKT_Time
        {
            StepMode = TimeStepMode.Resume,
            ServerTick = Master.GlobalServerTick
        };

        ServerNetwork.SendPacketToAllClients(PacketHeader.GameTime, broadcast);
    }

    public static void BroadcastTickHeartbeat()
    {
        PKT_Time data = new PKT_Time
        {
            StepMode = TimeStepMode.TickHeartbeat,
            ServerTick = Master.GlobalServerTick
        };

        ServerNetwork.SendPacketToAllClients(PacketHeader.GameTime, data);
    }
}
