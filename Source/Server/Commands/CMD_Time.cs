using GameServer.Core;
using GameServer.Managers;
using RTShared.Commands;
using RTShared.Misc;

namespace GameServer.Commands
{
    public class CMD_Time : CMD_Base
    {
        public CMD_Time()
        {
            Prefix = "time";
            Description = "Controls global time synchronization.";
            ParameterCount = -1;
        }

        public override void Action()
        {
            if (CommandParameters.Length < 1)
            {
                ShowStatus();
                return;
            }

            string subCommand = CommandParameters[0].ToLower();

            switch (subCommand)
            {
                case "speed":
                    HandleSpeed();
                    break;

                case "pause":
                    Master.GlobalTimePaused = true;
                    Master.GlobalTimePausedBy = "Console";
                    TickManager.AdvanceTick();
                    TickManager.SaveTimeState();
                    Printer.Warning("Global time paused via console.");
                    break;

                case "resume":
                    Master.GlobalTimePaused = false;
                    Master.GlobalTimePausedBy = null;
                    Master.TickBaseTimestamp = DateTime.UtcNow.Ticks;
                    TickManager.SaveTimeState();
                    Printer.Warning("Global time resumed via console.");
                    break;

                case "status":
                    ShowStatus();
                    break;

                default:
                    Printer.Warning($"Unknown time sub-command '{subCommand}'. Use: speed/pause/resume/status");
                    break;
            }
        }

        private static void HandleSpeed()
        {
            if (CommandParameters.Length < 2)
            {
                Printer.Warning("Usage: time speed <1|2|3>");
                return;
            }

            if (float.TryParse(CommandParameters[1], out float speed))
            {
                if (speed < 1f || speed > 3f)
                {
                    Printer.Warning("Speed must be between 1 and 3.");
                    return;
                }

                TickManager.AdvanceTick();
                Master.GlobalTimeSpeed = speed;
                Master.TickBaseTimestamp = DateTime.UtcNow.Ticks;
                TickManager.SaveTimeState();
                Printer.Warning($"Global speed set to {speed}x.");
            }
            else
            {
                Printer.Warning($"Invalid speed value '{CommandParameters[1]}'.");
            }
        }

        private static void ShowStatus()
        {
            Printer.Title("=== Global Time Status ===");
            Printer.Warning($"Tick:         {Master.GlobalServerTick}");
            Printer.Warning($"Speed:        {Master.GlobalTimeSpeed}x");
            Printer.Warning($"Paused:       {Master.GlobalTimePaused}");
            if (Master.GlobalTimePaused)
                Printer.Warning($"Paused By:    {Master.GlobalTimePausedBy}");
            Printer.Warning($"Heartbeat:    {Master.TickHeartbeatIntervalMs}ms");
            Printer.Title("==========================");
        }
    }
}
