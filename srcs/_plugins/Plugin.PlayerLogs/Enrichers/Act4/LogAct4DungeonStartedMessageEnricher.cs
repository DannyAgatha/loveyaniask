using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Act4;
using WingsEmu.Game.Act4.Event;

namespace Plugin.PlayerLogs.Enrichers.Act4
{
    public class LogAct4DungeonStartedMessageEnricher : ILogMessageEnricher<Act4DungeonStartedEvent, LogAct4DungeonStartedMessage>
    {
        public void Enrich(LogAct4DungeonStartedMessage message, Act4DungeonStartedEvent e)
        {
            message.DungeonType = e.DungeonType.ToString();
            message.FactionType = e.FactionType.ToString();
        }
    }
}