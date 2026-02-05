using System;
using ProtoBuf;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyAchievementCompletionDto
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public DateTime CompletionDate { get; set; }
}