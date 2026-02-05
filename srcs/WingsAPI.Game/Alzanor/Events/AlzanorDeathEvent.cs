using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorDeathEvent : PlayerEvent
{
    public IBattleEntity Killer;

public AlzanorDeathEvent(IBattleEntity killer)
{
    Killer = killer;
}
}