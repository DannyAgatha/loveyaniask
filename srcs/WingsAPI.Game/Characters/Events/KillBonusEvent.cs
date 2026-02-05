using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Characters.Events;

public class KillBonusEvent : PlayerEvent
{
    public IMonsterEntity MonsterEntity { get; set; }
    
    public bool IsByButcherCommand { get; init; }
}