// NosEmu
// 


using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Game.Relations;

public class AddRelationEvent : PlayerEvent
{
    public AddRelationEvent(long targetCharacterId, CharacterRelationType relationType)
    {
        TargetCharacterId = targetCharacterId;
        RelationType = relationType;
    }

    public long TargetCharacterId { get; }
    public CharacterRelationType RelationType { get; }
}