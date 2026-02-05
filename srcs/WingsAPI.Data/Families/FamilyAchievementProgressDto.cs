using ProtoBuf;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyAchievementProgressDto
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public int Count { get; set; }
}