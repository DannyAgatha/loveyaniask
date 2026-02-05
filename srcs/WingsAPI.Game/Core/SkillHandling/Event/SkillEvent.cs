using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Core.SkillHandling.Event;

public class SkillEvent : PlayerEvent
{
    public int SkillId { get; set; }

    public IBattleEntity Target { get; set; }

    public SkillInfo SkillInfo { get; set; }
}