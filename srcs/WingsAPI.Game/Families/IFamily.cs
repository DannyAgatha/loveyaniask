using System.Collections.Generic;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Families;

public interface IFamily
{
    long Id { get; set; }
    string Name { get; set; }
    byte Level { get; set; }
    long Experience { get; set; }
    byte Faction { get; set; }
    GenderType HeadGender { get; set; }
    string Message { get; set; }
    FamilyWarehouseAuthorityType AssistantWarehouseAuthorityType { get; set; }
    FamilyWarehouseAuthorityType MemberWarehouseAuthorityType { get; set; }
    bool AssistantCanGetHistory { get; set; }
    bool AssistantCanInvite { get; set; }
    bool AssistantCanNotice { get; set; }
    bool AssistantCanShout { get; set; }
    bool MemberCanGetHistory { get; set; }
    List<FamilyMembership> Members { get; }
    List<FamilyLogDto> Logs { get; }
    FamilyMembership Head { get; set; }
    Dictionary<int, FamilyUpgrade> Upgrades { get; }
    Dictionary<FamilyUpgradeType, (short, short?)> UpgradeValues { get; }
    Dictionary<int, FamilyMissionDto> Mission { get; }
    Dictionary<int, FamilyAchievementCompletionDto> Achievements { get; }
    Dictionary<int, FamilyAchievementProgressDto> AchievementProgress { get; }
    FamilyRankingDto PreviousMonthRankStat { get; set; }
    FamilyRankingDto CurrentDayRankStat { get; set; }
    FamilyRankingDto CurrentMonthRankStat { get; set; }
    FamilyRankingDto GlobalRankStat { get; set; }

    int GetMaximumMembershipCapacity();
    int GetWarehouseCapacity();
    bool HasAlreadyBoughtUpgrade(int upgradeId);
}