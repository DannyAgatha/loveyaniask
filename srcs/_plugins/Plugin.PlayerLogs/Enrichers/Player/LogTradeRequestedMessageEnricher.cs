using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Player;
using WingsEmu.Game.Exchange.Event;

namespace Plugin.PlayerLogs.Enrichers.Player
{
    public class LogTradeRequestedMessageEnricher : ILogMessageEnricher<TradeRequestedEvent, LogTradeRequestedMessage>
    {
        public void Enrich(LogTradeRequestedMessage message, TradeRequestedEvent e)
        {
            message.TargetId = e.TargetId;
        }
    }
}