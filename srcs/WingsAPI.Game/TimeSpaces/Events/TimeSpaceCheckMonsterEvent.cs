using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceCheckMonsterEvent : IAsyncEvent
{
    public TimeSpaceCheckMonsterEvent(IMonsterEntity monsterEntity) => MonsterEntity = monsterEntity;

    public IMonsterEntity MonsterEntity { get; }
}