using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Algorithm.Events;

public class GenerateExperienceEvent : IAsyncEvent
{
    public GenerateExperienceEvent(IPlayerEntity character, IMonsterEntity monsterEntity, long? monsterOwnerId)
    {
        Character = character;
        MonsterEntity = monsterEntity;
        MonsterOwnerId = monsterOwnerId;
    }

    public IPlayerEntity Character { get; }
    public IMonsterEntity MonsterEntity { get; }
    public long? MonsterOwnerId { get; }
}