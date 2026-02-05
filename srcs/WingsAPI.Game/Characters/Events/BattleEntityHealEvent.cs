using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Characters.Events;

public class BattleEntityHealEvent : IBattleEntityEvent
{
    public int HpHeal { get; init; }
    public int MpHeal { get; init; }
    public bool HealMates { get; init; }
    public IBattleEntity Entity { get; init; }
}