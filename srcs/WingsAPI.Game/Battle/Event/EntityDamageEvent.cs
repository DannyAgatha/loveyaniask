using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Battle;

public class EntityDamageEvent : IAsyncEvent
{
    public IBattleEntity Damaged { get; set; }
    public IBattleEntity Damager { get; set; }
    public int Damage { get; set; }
    public bool CanKill { get; set; }
    public SkillInfo SkillInfo { get; set; }
}