// NosEmu
// 


using ProtoBuf;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.DTOs.Relations;

[ProtoContract]
public class CharacterRelationDTO
{
    [ProtoMember(1)]
    public long CharacterId { get; set; }

    [ProtoMember(2)]
    public long RelatedCharacterId { get; set; }

    [ProtoMember(3)]
    public string RelatedName { get; set; }

    [ProtoMember(4)]
    public CharacterRelationType RelationType { get; set; }
}