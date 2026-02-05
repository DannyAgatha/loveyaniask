using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Revival;

public class RevivalStartProcedureEvent : PlayerEvent
{
    public RevivalStartProcedureEvent(IBattleEntity killer) => Killer = killer;
    public IBattleEntity Killer { get; }
}