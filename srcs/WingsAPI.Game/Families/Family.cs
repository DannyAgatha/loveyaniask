// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families;

public class Family : IFamily
{
    public Family(FamilyDTO input, IReadOnlyCollection<FamilyMembershipDto> members, List<FamilyLogDto> logs, ISessionManager sessionManager)
    {
        Members = new List<FamilyMembership>();
        Logs = logs ?? new List<FamilyLogDto>();
        Upgrades = new Dictionary<int, FamilyUpgrade>();
        UpgradeValues = new Dictionary<FamilyUpgradeType, (short, short?)>();
        Achievements = new Dictionary<int, FamilyAchievementCompletionDto>();
        AchievementProgress = new Dictionary<int, FamilyAchievementProgressDto>();
        Mission = new Dictionary<int, FamilyMissionDto>();
        if (input == null || members == null)
        {
            return;
        }

        FamilyMembership head = null;

        foreach (FamilyMembershipDto membershipDto in members)
        {
            var membership = new FamilyMembership(membershipDto, sessionManager);
            Members.Add(membership);

            if (membership.Authority == FamilyAuthority.Head)
            {
                head = membership;
            }
        }

        Experience = input.Experience;
        Head = head;
        HeadGender = input.HeadGender;
        Id = input.Id;
        Level = input.Level;
        Message = input.Message;
        Faction = input.Faction;
        AssistantWarehouseAuthorityType = input.AssistantWarehouseAuthorityType;
        AssistantCanGetHistory = input.AssistantCanGetHistory;
        AssistantCanInvite = input.AssistantCanInvite;
        AssistantCanNotice = input.AssistantCanNotice;
        AssistantCanShout = input.AssistantCanShout;
        MemberWarehouseAuthorityType = input.MemberWarehouseAuthorityType;
        MemberCanGetHistory = input.MemberCanGetHistory;
        Name = input.Name;
        CurrentMonthRankStat = input.CurrentMonthRankStat;
        GlobalRankStat = input.GlobalRankStat;
        PreviousMonthRankStat = input.PreviousMonthRankStat;
        CurrentDayRankStat = input.CurrentDayRankStat;

        if (input.Upgrades != null)
        {
            if (input.Upgrades.UpgradesBought != null)
            {
                foreach (int upgradeId in input.Upgrades.UpgradesBought)
                {
                    Upgrades[upgradeId] = new FamilyUpgrade { Id = upgradeId, State = FamilyUpgradeState.PASSIVE };
                }
            }

            if (input.Upgrades.UpgradeValues != null)
            {
                foreach ((FamilyUpgradeType upgradeType, (short value, short? secondValue)) in input.Upgrades.UpgradeValues)
                {
                    UpgradeValues[upgradeType] = (value, secondValue);
                }
            }
        }

        if (input.Achievements?.Achievements != null && input.Achievements.Achievements.Any())
        {
            foreach ((int achievementId, FamilyAchievementCompletionDto achievement) in input.Achievements.Achievements)
            {
                Achievements[achievementId] = achievement;
            }
        }

        if (input.Achievements?.Progress != null && input.Achievements.Progress.Any())
        {
            foreach ((int achievementId, FamilyAchievementProgressDto achievement) in input.Achievements.Progress)
            {
                AchievementProgress[achievementId] = achievement;
            }
        }

        if (input.Missions?.Missions != null && input.Missions.Missions.Any())
        {
            foreach ((int achievementId, FamilyMissionDto achievement) in input.Missions.Missions)
            {
                Mission[achievementId] = achievement;
            }
        }
    }

    public long Id { get; set; }

    public string Name { get; set; }

    public byte Level { get; set; }

    public long Experience { get; set; }

    public byte Faction { get; set; }

    public GenderType HeadGender { get; set; }

    public string Message { get; set; }

    public FamilyWarehouseAuthorityType AssistantWarehouseAuthorityType { get; set; }

    public FamilyWarehouseAuthorityType MemberWarehouseAuthorityType { get; set; }

    public bool AssistantCanGetHistory { get; set; }
    public bool AssistantCanInvite { get; set; }
    public bool AssistantCanNotice { get; set; }

    public bool AssistantCanShout { get; set; }
    public bool MemberCanGetHistory { get; set; }

    public List<FamilyMembership> Members { get; }

    public List<FamilyLogDto> Logs { get; }

    public FamilyMembership Head { get; set; }

    public Dictionary<FamilyUpgradeType, (short, short?)> UpgradeValues { get; }
    public Dictionary<int, FamilyMissionDto> Mission { get; }

    public int GetMaximumMembershipCapacity()
        => UpgradeValues != null && UpgradeValues.TryGetValue(FamilyUpgradeType.INCREASE_FAMILY_MEMBERS_LIMIT, out (short, short?) capacity) ? capacity.Item1 : 50;
    public int GetWarehouseCapacity() => UpgradeValues != null && UpgradeValues.TryGetValue(FamilyUpgradeType.INCREASE_FAMILY_WAREHOUSE, out (short, short?) capacity) ? capacity.Item1 : 0;

    public bool HasAlreadyBoughtUpgrade(int upgradeId) => Upgrades != null && Upgrades.ContainsKey(upgradeId);

    public Dictionary<int, FamilyUpgrade> Upgrades { get; }
    public Dictionary<int, FamilyAchievementCompletionDto> Achievements { get; set; }
    public Dictionary<int, FamilyAchievementProgressDto> AchievementProgress { get; set; }
    public FamilyRankingDto PreviousMonthRankStat { get; set; }
    public FamilyRankingDto CurrentMonthRankStat { get; set; }
    public FamilyRankingDto GlobalRankStat { get; set; }
    public FamilyRankingDto CurrentDayRankStat { get; set; }
}