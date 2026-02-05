using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class UseObjMinilandEvent : PlayerEvent
{
    public UseObjMinilandEvent(string characterName, short slot)
    {
        CharacterName = characterName;
        Slot = slot;
    }

    public string CharacterName { get; }

    public short Slot { get; }
}