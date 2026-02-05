using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Relations;

public class RemoveRelationEvent : PlayerEvent
{
    public RemoveRelationEvent(long targetCharacterId, CharacterRelationType type)
    {
        TargetCharacterId = targetCharacterId;
        Type = type;
    }

    public long TargetCharacterId { get; }
    public CharacterRelationType Type { get; }
}