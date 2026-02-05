using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public class LogBoxOpenedMessageEnricher : ILogMessageEnricher<BoxOpenedEvent, LogBoxOpenedMessage>
    {
        public void Enrich(LogBoxOpenedMessage message, BoxOpenedEvent e)
        {
            message.Box = e.Box;
            message.Rewards = e.Rewards;
        }
    }
}