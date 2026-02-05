using System.Collections.Generic;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Buffs.Events;

public class BuffAddEvent : IBattleEntityEvent
{
    public BuffAddEvent(IBattleEntity entity, IEnumerable<Buff> buffs)
    {
        Entity = entity;
        Buffs = buffs;
    }

    public IEnumerable<Buff> Buffs { get; }

    public IBattleEntity Entity { get; }
}