using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act7.Tattoos;

public class UpgradeTattooEvent : PlayerEvent
{
    public UpgradeTattooEvent(TattooUpgradeProtection upgradeProtection, IBattleEntitySkill tattooSkill)
    {
        UpgradeProtection = upgradeProtection;
        TattooSkill = tattooSkill;
    }

    public TattooUpgradeProtection UpgradeProtection { get; }
    public IBattleEntitySkill TattooSkill { get; }
}
