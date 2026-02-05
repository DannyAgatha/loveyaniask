// NosEmu
// 


using ProtoBuf;

namespace WingsEmu.DTOs.Buffs;

[ProtoContract]
public class CharacterStaticBuffDto
{
    [ProtoMember(1)]
    public long CharacterId { get; set; }

    [ProtoMember(2)]
    public int RemainingTime { get; set; }

    [ProtoMember(3)]
    public int CardId { get; set; }
}