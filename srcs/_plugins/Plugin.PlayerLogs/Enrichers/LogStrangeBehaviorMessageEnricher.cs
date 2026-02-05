using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers
{
    public class LogStrangeBehaviorMessageEnricher : ILogMessageEnricher<StrangeBehaviorEvent, LogStrangeBehaviorMessage>
    {
        public void Enrich(LogStrangeBehaviorMessage message, StrangeBehaviorEvent e)
        {
            message.SeverityType = e.Severity.ToString();
            message.Message = e.Reason;
        }
    }
}