using ProtoBuf;
using WingsAPI.Packets.Enums.Prestige;

namespace WingsAPI.Data.Prestige;

[ProtoContract]
public class PrestigeTaskDto
{
    [ProtoMember(1)]
    public PrestigeTaskType TaskType { get; set; }

    [ProtoMember(2)]
    public int? ItemVnum { get; set; }

    [ProtoMember(3)] 
    public int? MonsterVnum { get; set; }

    [ProtoMember(4)]
    public long RequiredAmount { get; set; }

    [ProtoMember(5)]
    public int? MapVnum { get; set; }

    [ProtoMember(6)]
    public int? RaidId { get; set; }

    [ProtoMember(7)]
    public int? LevelRangeMargin { get; set; }

    [ProtoMember(8)]
    public long Progress { get; set; } = 0;

    [ProtoIgnore]
    public bool Completed => Progress >= RequiredAmount;
}

