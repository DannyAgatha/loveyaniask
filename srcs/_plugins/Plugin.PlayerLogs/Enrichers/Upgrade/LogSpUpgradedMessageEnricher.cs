using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogSpUpgradedMessageEnricher : ILogMessageEnricher<SpUpgradedEvent, LogSpUpgradedMessage>
    {
        public void Enrich(LogSpUpgradedMessage message, SpUpgradedEvent e)
        {
            message.Sp = e.Sp;
            message.Mode = e.UpgradeMode.ToString();
            message.Result = e.UpgradeResult.ToString();
            message.OriginalUpgrade = e.OriginalUpgrade;
            message.IsProtected = e.IsProtected;
        }
    }
}