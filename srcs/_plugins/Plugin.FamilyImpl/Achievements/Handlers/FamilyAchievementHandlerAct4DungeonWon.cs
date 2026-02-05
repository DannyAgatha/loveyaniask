// NosEmu
// 


using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl.Achievements.Handlers
{
    public class FamilyAchievementHandlerAct4DungeonWon : IAsyncEventProcessor<Act4DungeonWonEvent>
    {
        private readonly IFamilyAchievementManager _familyManager;
        private readonly IFamilyMissionManager _familyMissionManager;

        public FamilyAchievementHandlerAct4DungeonWon(IFamilyAchievementManager familyManager, IFamilyMissionManager familyMissionManager)
        {
            _familyManager = familyManager;
            _familyMissionManager = familyMissionManager;
        }

        public async Task HandleAsync(Act4DungeonWonEvent e, CancellationToken cancellation)
        {
            DungeonInstance dungeon = e.DungeonInstance;
            long familyId = dungeon.FamilyId;
            DungeonType dungeonType = dungeon.DungeonType;
            if (!dungeon.PlayerDeathInBossRoom)
            {
                _familyMissionManager.IncrementFamilyMission(familyId, (int)FamilyMissionVnums.DAILY_DEFEAT_DUNGEON_BOSS_WITHOUT_DYING);
            }

            TimeSpan time = DateTime.UtcNow - dungeon.StartInBoosRoom;
            if (time < TimeSpan.FromMinutes(10))
            {
                _familyMissionManager.IncrementFamilyMission(familyId, (int)FamilyMissionVnums.DAILY_DEFEAT_DUNGEON_BOSS_LESS_10MIN);
            }

            _familyMissionManager.IncrementFamilyMission(familyId, (int)FamilyMissionVnums.DAILY_DEFEAT_ANY_ACT4_DUNGEON_1_TIME);
            _familyManager.IncrementFamilyAchievement(familyId, (int)FamilyAchievementsVnum.DEFEAT_ANY_ACT4_DUNGEON_10_TIMES);

            FamilyAchievementsVnum dungeonSpecificAchievement = dungeonType switch
            {
                DungeonType.Berios => FamilyAchievementsVnum.DEFEAT_BERIOS_ACT4_DUNGEON_1_TIME,
                DungeonType.Hatus => FamilyAchievementsVnum.DEFEAT_HATUS_ACT4_DUNGEON_1_TIME,
                DungeonType.Calvinas => FamilyAchievementsVnum.DEFEAT_CALVINAS_ACT4_DUNGEON_1_TIME,
                DungeonType.Morcos => FamilyAchievementsVnum.DEFEAT_MORCOS_ACT4_DUNGEON_1_TIME
            };

            _familyManager.IncrementFamilyAchievement(familyId, (int)dungeonSpecificAchievement);

            await e.DungeonLeader.EmitEventAsync(new Act4FamilyDungeonWonEvent
            {
                DungeonType = dungeonType,
                FamilyId = familyId,
                Members = e.Members
            });
        }
    }
}