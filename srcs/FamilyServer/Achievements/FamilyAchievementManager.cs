using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Logs;
using FamilyServer.Managers;
using Microsoft.Extensions.Hosting;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsEmu.Game.Families;
using WingsEmu.Packets.Enums.Families;

namespace FamilyServer.Achievements
{
    public class FamilyAchievementManager : BackgroundService
    {
        private readonly Dictionary<int, FamilyAchievementSpecificConfiguration> _achievementsConfiguration;
        private readonly ConcurrentQueue<FamilyAchievementIncrement> _achievementsQueue = new();
        private readonly FamilyAchievementsConfiguration _configuration;
        private readonly IExpirableLockService _expirableLockService;
        private readonly FamilyExperienceManager _familyExperienceManager;
        private readonly FamilyLogManager _familyLogManager;
        private readonly FamilyManager _familyManager;
        private readonly IFamilyService _familyService;
        private readonly IMessagePublisher<FamilyUpdateMessage> _familyUpdatePublisher;
        private readonly Dictionary<int, FamilyMissionSpecificConfiguration> _missionSpecificConfigurations;
        private readonly ConcurrentQueue<FamilyMissionIncrement> _missionsQueue = new();
        private readonly IMessagePublisher<FamilyAchievementUnlockedMessage> _unlockedMessagePublisher;

        public FamilyAchievementManager(FamilyManager familyManager, FamilyAchievementsConfiguration configuration, IMessagePublisher<FamilyUpdateMessage> familyUpdatePublisher,
            IMessagePublisher<FamilyAchievementUnlockedMessage> unlockedMessagePublisher, IFamilyService familyService, FamilyMissionsConfiguration missionsConfiguration,
            IExpirableLockService expirableLockService, FamilyExperienceManager familyExperienceManager, FamilyLogManager familyLogManager)
        {
            _familyManager = familyManager;
            _configuration = configuration;
            _familyUpdatePublisher = familyUpdatePublisher;
            _unlockedMessagePublisher = unlockedMessagePublisher;
            _familyService = familyService;
            _expirableLockService = expirableLockService;
            _familyExperienceManager = familyExperienceManager;
            _familyLogManager = familyLogManager;
            _missionSpecificConfigurations = missionsConfiguration.ToDictionary(s => s.MissionId);
            _achievementsConfiguration = configuration.Counters.ToDictionary(s => s.Id);
        }

        private static TimeSpan RefreshTime => TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("FAMILY_ACHIEVEMENT_REFRESH_IN_SECONDS") ?? "5"));

        public void AddIncrementToQueue(FamilyAchievementIncrement message)
        {
            _achievementsQueue.Enqueue(message);
        }

        public void AddIncrementToQueue(FamilyMissionIncrement message)
        {
            _missionsQueue.Enqueue(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Dictionary<long, FamilyDTO> toUpdate = new();
                await FlushPendingAchievements(toUpdate);
                await FlushPendingMissions(toUpdate);

                if (toUpdate.Count > 0)
                {
                    foreach (FamilyDTO family in toUpdate.Values)
                    {
                        await _familyManager.SaveFamilyAsync(family);
                    }

                    await _familyUpdatePublisher.PublishAsync(new FamilyUpdateMessage
                    {
                        Families = toUpdate.Values.ToList(),
                        ChangedInfoFamilyUpdate = ChangedInfoFamilyUpdate.AchievementsAndMissions
                    });
                }

                await Task.Delay(RefreshTime, stoppingToken);
            }
        }

        private async Task FlushPendingMissions(Dictionary<long, FamilyDTO> toUpdate)
        {
            try
            {
                if (_missionsQueue.IsEmpty)
                {
                    return;
                }

                HashSet<(long, int)> unlockedMissions = new();
                while (_missionsQueue.TryDequeue(out FamilyMissionIncrement message))
                {
                    FamilyDTO family = await _familyManager.GetFamilyByFamilyIdAsync(message.FamilyId);

                    if (family == null)
                    {
                        Log.Debug("Family not found");
                        continue;
                    }

                    int missionId = message.MissionId;
                    int value = message.ValueToAdd;
                    long? characterId = message.CharacterId;
                    Log.Debug($"[FAMILY_MISSIONS] Family {family.Id} adding {value} to mission: {missionId}");

                    family.Missions ??= new FamilyMissionsDto();
                    family.Missions.Missions ??= new Dictionary<int, FamilyMissionDto>();

                    if (!family.Missions.Missions.TryGetValue(missionId, out FamilyMissionDto progress))
                    {
                        family.Missions.Missions[missionId] = progress = new FamilyMissionDto
                        {
                            Id = missionId
                        };
                    }

                    if (progress.CompletionDate.HasValue && progress.CompletionDate < DateTime.UtcNow)
                    {
                        Log.Debug($"[FAMILY_MISSIONS] {progress.CompletionDate.Value} need to wait reset");
                        continue;
                    }

                    if (!_missionSpecificConfigurations.TryGetValue(missionId, out FamilyMissionSpecificConfiguration config))
                    {
                        Log.Error($"Family achievement config {missionId} is not configured", new Exception());
                        continue;
                    }

                    if (config.OncePerPlayerPerDay && !characterId.HasValue)
                    {
                        Log.Debug($"[FAMILY_MISSION] could not increment mission: {missionId} for family: {family.Id} because characterId is missing");
                        continue;
                    }

                    if (config.OncePerPlayerPerDay &&
                        !await _expirableLockService.TryAddTemporaryLockAsync($"game:locks:family:{family.Id}:missions:{missionId}:character:{characterId}", DateTime.UtcNow.Date.AddDays(1)))
                    {
                        Log.Debug($"[FAMILY_MISSION] {characterId} could not increment mission: {missionId} for family: {family.Id}, already done for today");
                        continue;
                    }

                    progress.Count += value;
                    toUpdate.TryAdd(family.Id, family);
                    if (progress.Count < config.Value)
                    {
                        Log.Debug($"[FAMILY_MISSIONS] Family {family.Id} tried to increment mission: {missionId} progress.Count < config.Value");
                        continue;
                    }

                    // mission done
                    DateTime now = DateTime.UtcNow;
                    if ((FamilyMissionVnums)progress.Id != FamilyMissionVnums.MONTHLY_FIRST_FXP_RANKING &&
                        (FamilyMissionVnums)progress.Id != FamilyMissionVnums.MONTHLY_ANGEL_FIRST_FXP_RANKING &&
                        (FamilyMissionVnums)progress.Id != FamilyMissionVnums.MONTHLY_DEMON_FIRST_FXP_RANKING)
                    {
                        progress.CompletionDate = now;
                    }

                    progress.CompletionCount++;
                    unlockedMissions.Add((family.Id, missionId));

                    if (config.Rewards != null)
                    {
                        foreach (FamilyMissionReward reward in config.Rewards)
                        {
                            if (reward.FamilyXp.HasValue)
                            {
                                _familyExperienceManager.AddExperienceIncrementRequest(new ExperienceIncrementRequest
                                {
                                    FamilyId = family.Id,
                                    Experience = reward.FamilyXp.Value
                                });
                            }
                        }
                    }

                    if (progress.Count > config.Value)
                    {
                        progress.Count = config.Value;
                        continue;
                    }

                    Log.Debug($"[FAMILY_MISSIONS] Family {family.Id} tried to increment mission: {missionId} progress.Count <= config.Value");
                }

                foreach ((long familyId, int missionId) in unlockedMissions)
                {
                    _familyLogManager.SaveFamilyLogs(new[]
                    {
                        new FamilyLogDto
                        {
                            FamilyLogType = FamilyLogType.FamilyMission,
                            FamilyId = familyId,
                            Actor = missionId.ToString(),
                            Timestamp = DateTime.UtcNow
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_ACHIEVEMENT_MANAGER]", e);
            }
        }

        private async Task FlushPendingAchievements(Dictionary<long, FamilyDTO> toUpdate)
        {
            try
            {
                if (_achievementsQueue.IsEmpty)
                {
                    return;
                }

                HashSet<(long, int)> unlockedAchievements = new();
                while (_achievementsQueue.TryDequeue(out FamilyAchievementIncrement message))
                {
                    FamilyDTO family = await _familyManager.GetFamilyByFamilyIdAsync(message.FamilyId);

                    if (family == null)
                    {
                        Log.Debug("Family not found");
                        continue;
                    }

                    int achievementId = message.AchievementId;
                    int value = message.ValueToAdd;
                    Log.Debug($"[FAMILY_ACHIEVEMENT] Family {family.Id} adding {value} to achievement: {achievementId}");


                    family.Achievements ??= new FamilyAchievementsDto();
                    family.Achievements.Achievements ??= new Dictionary<int, FamilyAchievementCompletionDto>();
                    family.Achievements.Progress ??= new Dictionary<int, FamilyAchievementProgressDto>();
                    if (family.Achievements?.Achievements?.ContainsKey(achievementId) == true)
                    {
                        AddNextAchievement(achievementId, family, value);
                        continue;
                    }

                    if (!family.Achievements.Progress.TryGetValue(achievementId, out FamilyAchievementProgressDto progress))
                    {
                        family.Achievements.Progress[achievementId] = progress = new FamilyAchievementProgressDto
                        {
                            Id = achievementId
                        };
                    }
                    foreach (FamilyAchievementSpecificConfiguration achievement in _achievementsConfiguration.Values.Where(achievement => achievement.RequiredLevel.HasValue
                          && achievement.RequiredLevel.Value <= family.Level
                          && !family.Achievements.Progress.ContainsKey(achievement.Id)
                          && !family.Achievements.Achievements.ContainsKey(achievement.Id)))
                    {
                        family.Achievements.Progress[achievement.Id] = new FamilyAchievementProgressDto
                        {
                            Id = achievement.Id,
                            Count = 0
                        };
                        Log.Debug($"[FAMILY_ACHIEVEMENT] Injected achievement {achievement.Id} for family {family.Id} based on RequiredLevel {achievement.RequiredLevel.Value}.");
                    }
                    if (!_achievementsConfiguration.TryGetValue(achievementId, out FamilyAchievementSpecificConfiguration config))
                    {
                        Log.Error($"Family achievement config {achievementId} is not configured", new Exception($"[FAMILY_ACHIEVEMENT_CONFIG] {progress.Id} not configured"));
                        continue;
                    }

                    if (config.RequiredId.HasValue && !family.Achievements.Achievements.ContainsKey(config.RequiredId.Value))
                    {
                        Log.Debug($"[FAMILY_ACHIEVEMENT] Family {family.Id} tried to increment achievement: {achievementId} but didn't have {config.RequiredId.Value}");
                        continue;
                    }

                    progress.Count += value;

                    toUpdate.TryAdd(family.Id, family);
                    if (progress.Count < config.Value)
                    {
                        Log.Debug($"[FAMILY_ACHIEVEMENT] Family {family.Id} tried to increment achievement: {achievementId} progress.Count < config.Value");
                        continue;
                    }

                    // achievement unlocked
                    family.Achievements.Achievements[achievementId] = new FamilyAchievementCompletionDto
                    {
                        Id = progress.Id,
                        CompletionDate = DateTime.UtcNow
                    };

                    (long Id, int achievementId) key = (family.Id, achievementId);
                    if (!unlockedAchievements.Contains(key))
                    {
                        unlockedAchievements.Add(key);
                    }

                    if (config.Rewards != null)
                    {
                        foreach (FamilyAchievementReward reward in config.Rewards)
                        {
                            if (reward.FamilyXp.HasValue)
                            {
                                _familyExperienceManager.AddExperienceIncrementRequest(new ExperienceIncrementRequest
                                {
                                    FamilyId = family.Id,
                                    Experience = reward.FamilyXp.Value
                                });
                            }

                            if (reward.FamilyUpgradeCategory.HasValue && reward.UpgradeValue.HasValue && reward.UpgradeId.HasValue)
                            {
                                await _familyService.TryAddFamilyUpgrade(new FamilyUpgradeRequest
                                {
                                    FamilyId = family.Id,
                                    FamilyUpgradeType = reward.FamilyUpgradeCategory.Value,
                                    UpgradeId = reward.UpgradeId.Value,
                                    Value = reward.UpgradeValue.Value,
                                    SecondValue = reward.UpgradeSecondValue.HasValue ? reward.UpgradeSecondValue.Value : null
                                });
                            }
                        }
                    }

                    family.Achievements.Progress.Remove(achievementId);

                    int progressCount = achievementId is >= 9018 and <= 9036 ? 0 : progress.Count;

                    AddNextAchievement(achievementId, family, progressCount);
                }

                foreach ((long familyId, int achievementId) in unlockedAchievements)
                {
                    await _unlockedMessagePublisher.PublishAsync(new FamilyAchievementUnlockedMessage
                    {
                        FamilyId = familyId,
                        AchievementId = achievementId
                    });

                    _familyLogManager.SaveFamilyLogs(new[]
                    {
                        new FamilyLogDto
                        {
                            FamilyLogType = FamilyLogType.FamilyAchievement,
                            FamilyId = familyId,
                            Actor = achievementId.ToString(),
                            Timestamp = DateTime.UtcNow
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_ACHIEVEMENT_MANAGER]", e);
            }
        }

        private void AddNextAchievement(int achievementId, FamilyDTO family, int value)
        {
            FamilyAchievementSpecificConfiguration tmp = _configuration.Counters.FirstOrDefault(s => s.RequiredId == achievementId);
            if (tmp == null)
            {
                Log.Error($"[FAMILY_ACHIEVEMENT_MANAGER] Couldn't find next achievement for {achievementId} - familyId: {family.Id}", new Exception());
                return;
            }

            _achievementsQueue.Enqueue(new FamilyAchievementIncrement
            {
                FamilyId = family.Id,
                AchievementId = tmp.Id,
                ValueToAdd = value
            });
        }
    }
}