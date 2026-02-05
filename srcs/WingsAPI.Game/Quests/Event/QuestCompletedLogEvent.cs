using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Quests.Event;

public class QuestCompletedLogEvent : PlayerEvent
{
    public CharacterQuest CharacterQuest { get; init; }
    public Location Location { get; init; }
}