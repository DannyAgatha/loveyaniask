using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;

namespace WingsEmu.Game.Buffs.Events;

public class AngelSpecialistElementalBuffEvent : PlayerEvent
{
    public SkillInfo Skill { get; set; }
}