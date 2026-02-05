// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Achievements;
using FamilyServer.Logs;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Packets.Enums.Families;

namespace FamilyServer.Managers
{
    public class FamilyExperienceManager : BackgroundService
    {
        private static readonly TimeSpan RefreshDelay = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("FAMILY_EXPERIENCE_REFRESH_DELAY") ?? "5"));

        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly ConcurrentQueue<ExperienceIncrementRequest> _expIncrementQueue = new();
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly FamilyLogManager _familyLogManager;
        private readonly FamilyManager _familyManager;
        private readonly FamilyMembershipManager _familyMembershipManager;
        private readonly IMessagePublisher<FamilyMemberUpdateMessage> _messagePublisher;
        private readonly IMessagePublisher<FamilyUpdateMessage> _messagePublisherLevelUp;
        private readonly FamilyMissionsConfiguration _missionsConfiguration;

        public FamilyExperienceManager(IMessagePublisher<FamilyUpdateMessage> messagePublisherLevelUp, IMessagePublisher<FamilyMemberUpdateMessage> messagePublisher,
            FamilyMembershipManager familyMembershipManager, FamilyManager familyManager, FamilyConfiguration familyConfiguration, IAsyncEventPipeline eventPipeline,
            FamilyMissionsConfiguration missionsConfiguration, FamilyLogManager familyLogManager)
        {
            _messagePublisherLevelUp = messagePublisherLevelUp;
            _messagePublisher = messagePublisher;
            _familyMembershipManager = familyMembershipManager;
            _familyManager = familyManager;
            _familyConfiguration = familyConfiguration;
            _eventPipeline = eventPipeline;
            _missionsConfiguration = missionsConfiguration;
            _familyLogManager = familyLogManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingExpRequests(stoppingToken);
                }
                catch (Exception e)
                {
                    Log.Error("[FAMILY_EXPERIENCE_MANAGER]", e);
                }

                await Task.Delay(RefreshDelay, stoppingToken);
            }
        }

        public void AddExperienceIncrementRequest(ExperienceIncrementRequest request)
        {
            _expIncrementQueue.Enqueue(request);
        }

        private async Task ProcessPendingExpRequests(CancellationToken cancellationToken)
        {
            if (_expIncrementQueue.IsEmpty)
            {
                return;
            }

            var memberships = new List<FamilyMembershipDto>();
            var families = new HashSet<FamilyDTO>();
            long? isFirstPlaceFamilyId = (await _familyManager.GetFamiliesAsync()).MaxBy(x => x?.PreviousMonthRankStat.ExpEarned)?.Id;
            
            while (_expIncrementQueue.TryDequeue(out ExperienceIncrementRequest request))
            {
                long? characterId = request.CharacterId;
                long? familyId = request.FamilyId;
                long experience = request.Experience;
                if (characterId.HasValue)
                {
                    FamilyMembershipDto membership = await _familyMembershipManager.GetFamilyMembershipByCharacterIdAsync(characterId.Value);
                    if (membership == null)
                    {
                        continue;
                    }

                    familyId = membership.FamilyId;
                    membership.Experience += experience;
                    if (!memberships.Contains(membership))
                    {
                        memberships.Add(membership);
                    }
                }

                if (!familyId.HasValue)
                {
                    // should not happen
                    continue;
                }

                FamilyDTO family = await _familyManager.GetFamilyByFamilyIdAsync(familyId.Value);
                if (family == null)
                {
                    continue;
                }

                family.Experience += experience;
                
                if (isFirstPlaceFamilyId.HasValue && family.Id == isFirstPlaceFamilyId.Value)
                {
                    family.CurrentDayRankStat.ExpEarned += (long)(experience * 0.90f);
                }

                if (!families.Contains(family))
                {
                    families.Add(family);
                }
            }

            await _familyMembershipManager.SaveFamilyMembershipsAsync(memberships);

            await _messagePublisher.PublishAsync(new FamilyMemberUpdateMessage
            {
                UpdatedMembers = memberships,
                ChangedInfoMemberUpdate = ChangedInfoMemberUpdate.Experience
            }, cancellationToken);

            foreach (FamilyDTO family in families)
            {
                byte oldLevel = family.Level;
                byte expectedLevel = _familyConfiguration.GetLevelByFamilyXp(family.Experience);
                if (expectedLevel == default)
                {
                    Log.Warn(
                        $"Found a family that exceeds the expected XP values. FamilyId: {family.Id.ToString()} | CurrentFamilyLvl: {family.Level.ToString()} | FamilyExperience: {family.Experience.ToString()}");
                    continue;
                }

                if (family.Level > expectedLevel)
                {
                    Log.Warn(
                        $"A family should have a lower level than it has (based in its experience). FamilyId: {family.Id.ToString()} | CurrentFamilyLvl: {family.Level.ToString()} | FamilyExperience: {family.Experience.ToString()} | ExpectedLvl: {expectedLevel}");
                    continue;
                }

                family.Level = expectedLevel;
                if (expectedLevel <= oldLevel)
                {
                    continue;
                }

                _familyLogManager.SaveFamilyLogs(new[]
                {
                    new FamilyLogDto
                    {
                        Actor = family.Level.ToString(),
                        FamilyId = family.Id,
                        Timestamp = DateTime.UtcNow,
                        FamilyLogType = FamilyLogType.FamilyLevelUp
                    }
                });

                await _eventPipeline.ProcessEventAsync(new FamilyAchievementIncrement
                {
                    AchievementId = (short)FamilyAchievementsVnum.FAMILY_LEVEL_2_UNLOCKED,
                    FamilyId = family.Id,
                    ValueToAdd = expectedLevel - oldLevel
                });

                family.Missions ??= new FamilyMissionsDto();
                family.Missions.Missions ??= new Dictionary<int, FamilyMissionDto>();

                IEnumerable<FamilyMissionSpecificConfiguration> missionsToAdd = _missionsConfiguration.Where(x => x.MinimumRequiredLevel <= family.Level);
                foreach (FamilyMissionSpecificConfiguration mission in missionsToAdd)
                {
                    int missionId = mission.MissionId;

                    if (family.Missions.Missions.TryGetValue(missionId, out _))
                    {
                        continue;
                    }

                    family.Missions.Missions[missionId] = new FamilyMissionDto
                    {
                        Id = missionId
                    };
                }
            }

            foreach (FamilyDTO family in families)
            {
                await _familyManager.SaveFamilyAsync(family);
            }

            await _messagePublisherLevelUp.PublishAsync(new FamilyUpdateMessage
            {
                Families = families,
                ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.Experience
            }, cancellationToken);
        }
    }
}