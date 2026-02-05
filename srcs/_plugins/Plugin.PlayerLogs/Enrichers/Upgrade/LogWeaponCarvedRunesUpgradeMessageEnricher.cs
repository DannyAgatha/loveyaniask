using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Upgrade;
using WingsEmu.Game.Act7.CarvedRunes;

namespace Plugin.PlayerLogs.Enrichers.Upgrade
{
    public class LogWeaponCarvedRunesMessageEnricher : ILogMessageEnricher<WeaponCarvedRuneUpgradedEvent, LogWeaponCarvedRunesUpgradeMessage>
    {
        public void Enrich(LogWeaponCarvedRunesUpgradeMessage message, WeaponCarvedRuneUpgradedEvent e)
        {
            message.Weapon = e.Weapon;
            message.Result = e.UpgradeResult.ToString();
            message.OriginalUpgrade = e.OriginalUpgrade;
            message.IsProtected = e.IsProtected;
        }
    }
}