using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceBonusMonsterEvent : IAsyncEvent
{
    public IReadOnlyList<IMonsterEntity> MonsterEntities { get; init; }
    public TimeSpaceSubInstance TimeSpaceSubInstance { get; init; }
}