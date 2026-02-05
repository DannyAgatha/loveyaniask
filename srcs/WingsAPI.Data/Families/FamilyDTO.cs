// NosEmu
// 


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyDTO : ILongDto
{
    [ProtoMember(2)]
    [MinLength(3)]
    [MaxLength(20)]
    public string Name { get; set; }

    [ProtoMember(3)]
    public byte Level { get; set; }

    [ProtoMember(4)]
    public long Experience { get; set; }

    [ProtoMember(5)]
    public byte Faction { get; set; }

    [ProtoMember(6)]
    public GenderType HeadGender { get; set; }

    [ProtoMember(9)]
    [MaxLength(50)]
    public string Message { get; set; }

    [ProtoMember(10)]
    public FamilyWarehouseAuthorityType AssistantWarehouseAuthorityType { get; set; }

    [ProtoMember(11)]
    public FamilyWarehouseAuthorityType MemberWarehouseAuthorityType { get; set; }

    [ProtoMember(12)]
    public bool AssistantCanGetHistory { get; set; }

    [ProtoMember(13)]
    public bool AssistantCanInvite { get; set; }

    [ProtoMember(14)]
    public bool AssistantCanNotice { get; set; }

    [ProtoMember(15)]
    public bool AssistantCanShout { get; set; }

    [ProtoMember(16)]
    public bool MemberCanGetHistory { get; set; }

    [ProtoMember(17)]
    public FamilyUpgradeDto Upgrades { get; set; }

    [ProtoMember(18)]
    public FamilyAchievementsDto Achievements { get; set; }

    [ProtoMember(19)]
    public FamilyMissionsDto Missions { get; set; }

    [ProtoMember(20)]
    public FamilyRankingDto PreviousMonthRankStat { get; set; }

    [ProtoMember(21)]
    public FamilyRankingDto CurrentMonthRankStat { get; set; }

    [ProtoMember(22)]
    public FamilyRankingDto GlobalRankStat { get; set; }

    [ProtoMember(23)]
    public FamilyRankingDto CurrentDayRankStat { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ProtoMember(1)]
    public long Id { get; set; }
}