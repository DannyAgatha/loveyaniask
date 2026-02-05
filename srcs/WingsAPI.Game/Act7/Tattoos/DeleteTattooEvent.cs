using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Skills;

namespace WingsEmu.Game.Act7.Tattoos;

public class DeleteTattooEvent : PlayerEvent
{
    public DeleteTattooEvent(IBattleEntitySkill tattooSkill) => TattooSkill = tattooSkill;
    public IBattleEntitySkill TattooSkill { get; set; }
}
