using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Act4;
using WingsEmu.Game.Revival;

namespace Plugin.PlayerLogs.Enrichers.Act4
{
    public class LogAct4PvpKillMessageEnricher : ILogMessageEnricher<Act4KillEvent, LogAct4PvpKillMessage>
    {
        public void Enrich(LogAct4PvpKillMessage message, Act4KillEvent e)
        {
            message.TargetId = e.TargetId;
            message.KillerFaction = e.Sender.PlayerEntity.Faction.ToString();
        }
    }
}