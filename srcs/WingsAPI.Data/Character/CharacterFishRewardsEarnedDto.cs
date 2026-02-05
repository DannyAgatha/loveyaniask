using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class CharacterFishRewardsEarnedDto
{
    [ProtoMember(1)]
    public int Vnum { get; set; }
}