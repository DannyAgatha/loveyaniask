using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Mates.Events;

public class MateDeathEvent : PlayerEvent
{
    public MateDeathEvent(IBattleEntity killer, IMateEntity mateEntity)
    {
        Killer = killer;
        MateEntity = mateEntity;
    }

    public IBattleEntity Killer { get; set; }

    public IMateEntity MateEntity { get; set; }
}