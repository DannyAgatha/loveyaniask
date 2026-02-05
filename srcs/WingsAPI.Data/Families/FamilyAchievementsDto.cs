using System;
using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyAchievementsDto
{
    [ProtoMember(1)]
    public Dictionary<int, FamilyAchievementCompletionDto> Achievements { get; set; }

    [ProtoMember(2)]
    public Dictionary<int, FamilyAchievementProgressDto> Progress { get; set; }
}

[ProtoContract]
public class FamilyMissionsDto
{
    [ProtoMember(1)]
    public Dictionary<int, FamilyMissionDto> Missions { get; set; }
}

[ProtoContract]
public class FamilyMissionDto
{
    [ProtoMember(1)]
    public int Id { get; set; }

    [ProtoMember(2)]
    public int Count { get; set; }

    [ProtoMember(3)]
    public int CompletionCount { get; set; }

    [ProtoMember(4)]
    public DateTime? CompletionDate { get; set; }
}