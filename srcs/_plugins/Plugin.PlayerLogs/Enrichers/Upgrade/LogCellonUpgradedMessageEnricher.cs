using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogCellonUpgradedMessageEnricher : ILogMessageEnricher<CellonUpgradedEvent, LogCellonUpgradedMessage>
    {
        public void Enrich(LogCellonUpgradedMessage message, CellonUpgradedEvent e)
        {
            message.Item = e.Item;
            message.CellonVnum = e.CellonVnum;
            message.Succeed = e.Succeed;
        }
    }
}