using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class CharacterFishDto
{
    [ProtoMember(1)]
    public long Amount { get; set; }

    [ProtoMember(2)]
    public long FishVnum { get; set; }

    [ProtoMember(3)]
    public double MaxLenght { get; set; }
}