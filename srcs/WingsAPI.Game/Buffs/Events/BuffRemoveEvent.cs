using System.Collections.Generic;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Buffs.Events;

public class BuffRemoveEvent : IBattleEntityEvent
{
    public IEnumerable<Buff> Buffs { get; init; }
    public bool RemovePermanentBuff { get; init; }
    public bool ShowMessage { get; init; } = true;
    public bool RemoveFromGroupId { get; init; } = true;
    public IBattleEntity Entity { get; init; }
}