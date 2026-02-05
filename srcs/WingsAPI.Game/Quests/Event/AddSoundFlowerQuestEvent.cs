using WingsEmu.Game._enum;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Quests.Event;

public class AddSoundFlowerQuestEvent : PlayerEvent
{
    public SoundFlowerType SoundFlowerType { get; init; }
}