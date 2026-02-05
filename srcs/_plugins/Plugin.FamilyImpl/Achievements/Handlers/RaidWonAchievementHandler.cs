using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Families;
using WingsEmu.Game.Raids.Events;

namespace Plugin.FamilyImpl.Achievements.Handlers;

public class RaidWonAchievementHandler : IAsyncEventProcessor<RaidWonEvent>
{
    private readonly IFamilyMissionManager _familyMissionManager;

    public RaidWonAchievementHandler(IFamilyMissionManager familyMissionManager) => _familyMissionManager = familyMissionManager;

    public async Task HandleAsync(RaidWonEvent e, CancellationToken cancellation)
    {
        IPlayerEntity player = e.Sender.PlayerEntity;
        if (!player.IsInFamily())
        {
            return;
        }

        long familyId = player.Family.Id;
        RaidType raidType = player.Raid.Type;

        FamilyMissionVnums? dungeonSpecificAchievement = raidType switch
        {
            RaidType.Cuby => FamilyMissionVnums.DAILY_DEFEAT_5_CUBY_RAID,
            RaidType.Ginseng => FamilyMissionVnums.DAILY_DEFEAT_5_GINSENG_RAID,
            RaidType.Castra => FamilyMissionVnums.DAILY_DEFEAT_5_CASTRA_RAID,
            RaidType.GiantBlackSpider => FamilyMissionVnums.DAILY_DEFEAT_5_GIANT_SPIDER_RAID,
            RaidType.Slade => FamilyMissionVnums.DAILY_DEFEAT_5_GIANT_SLADE_RAID,
            RaidType.RobberGang => FamilyMissionVnums.DAILY_DEFEAT_5_ROBBER_GANG_RAID,
            RaidType.Kertos => FamilyMissionVnums.DAILY_DEFEAT_5_KERTOS_RAID,
            RaidType.Valakus => FamilyMissionVnums.DAILY_DEFEAT_5_VALAKUS_RAID,
            RaidType.Grenigas => FamilyMissionVnums.DAILY_DEFEAT_5_GRENIGAS_RAID,
            RaidType.Namaju => FamilyMissionVnums.DAILY_DEFEAT_5_NAMAJU_RAID,
            _ => null
        };

        if (dungeonSpecificAchievement is null)
        {
            return;
        }

        _familyMissionManager.IncrementFamilyMission(familyId, player.Id, (int)dungeonSpecificAchievement);
    }
}