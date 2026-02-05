using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogSpPerfectedMessageEnricher : ILogMessageEnricher<SpPerfectedEvent, LogSpPerfectedMessage>
    {
        public void Enrich(LogSpPerfectedMessage message, SpPerfectedEvent e)
        {
            message.Success = e.Success;
            message.Sp = e.Sp;
            message.SpPerfectionLevel = e.SpPerfectionLevel;
        }
    }
}