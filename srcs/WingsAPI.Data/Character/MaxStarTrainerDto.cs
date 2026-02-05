using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class MaxStarTrainerDto
{
    [ProtoMember(1)]
    public long MonsterVnum { get; set; }

    [ProtoMember(2)]
    public long Stars { get; set; }

    [ProtoMember(3)]
    public byte Level { get; set; }
}