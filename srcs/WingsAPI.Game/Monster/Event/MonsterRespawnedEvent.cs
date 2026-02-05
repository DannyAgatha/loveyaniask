using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Monster.Event;

public class MonsterRespawnedEvent : IAsyncEvent
{
    public IMonsterEntity Monster { get; init; }
}