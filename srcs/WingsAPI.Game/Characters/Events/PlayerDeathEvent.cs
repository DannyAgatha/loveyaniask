using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Characters.Events;

public class PlayerDeathEvent : PlayerEvent
{
    public IBattleEntity Killer { get; set; }
}