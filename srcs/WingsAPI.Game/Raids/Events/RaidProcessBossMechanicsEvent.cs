using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Raids.Events;

public class RaidProcessBossMechanicsEvent : IAsyncEvent
{
    public IBattleEntity BattleEntity { get; init; }
    public SkillInfo SkillInfo { get; init; }
}