using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class CharacterRaidRestrictionDto
{
    [ProtoMember(1)]
    public byte LordDraco { get; set; }

    [ProtoMember(2)]
    public byte Glacerus { get; set; }
}