using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Monster.Event;

public class MonsterDeathEvent : IAsyncEvent
{
    public MonsterDeathEvent(IMonsterEntity monsterEntity) => MonsterEntity = monsterEntity;

    public IMonsterEntity MonsterEntity { get; }

    public IBattleEntity Killer { get; set; }

    public bool IsByCommand { get; init; }
}