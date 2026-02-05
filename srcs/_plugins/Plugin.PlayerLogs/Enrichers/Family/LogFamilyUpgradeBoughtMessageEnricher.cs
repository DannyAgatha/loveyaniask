using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public class LogFamilyUpgradeBoughtMessageEnricher : ILogMessageEnricher<FamilyUpgradeBoughtEvent, LogFamilyUpgradeBoughtMessage>
    {
        public void Enrich(LogFamilyUpgradeBoughtMessage message, FamilyUpgradeBoughtEvent e)
        {
            message.FamilyId = e.FamilyId;
            message.UpgradeValue = e.UpgradeValue;
            message.UpgradeVnum = e.UpgradeVnum;
            message.FamilyUpgradeType = e.FamilyUpgradeType.ToString();
        }
    }
}