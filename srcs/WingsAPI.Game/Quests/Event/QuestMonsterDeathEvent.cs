using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Quests.Event;

public class QuestMonsterDeathEvent : PlayerEvent
{
    public IMonsterEntity MonsterEntity { get; init; }
}