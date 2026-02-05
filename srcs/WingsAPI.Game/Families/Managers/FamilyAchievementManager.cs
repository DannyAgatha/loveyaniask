using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;

namespace Plugin.FamilyImpl.Achievements;

public class FamilyAchievementManager : BackgroundService, IFamilyAchievementManager, IFamilyMissionManager
{
    private readonly IMessagePublisher<FamilyAchievementIncrementMessage> _messagePublisher;
    private readonly IMessagePublisher<FamilyMissionIncrementMessage> _missionMessagePublisher;
    private readonly ConcurrentQueue<FamilyAchievementIncrementMessage> _pendingMessages = new();

    private readonly ConcurrentQueue<FamilyMissionIncrementMessage> _pendingMissionMessages = new();

    public FamilyAchievementManager(IMessagePublisher<FamilyAchievementIncrementMessage> messagePublisher, IMessagePublisher<FamilyMissionIncrementMessage> missionMessagePublisher)
    {
        _messagePublisher = messagePublisher;
        _missionMessagePublisher = missionMessagePublisher;
    }

    private static TimeSpan RefreshDelay => TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("FAMILY_ACHIEVEMENT_REFRESH_IN_SECONDS") ?? "5"));

    public void IncrementFamilyAchievement(long familyId, int achievementId, int counterToAdd)
    {
        // later
        _pendingMessages.Enqueue(new FamilyAchievementIncrementMessage
        {
            FamilyId = familyId,
            AchievementId = achievementId,
            ValueToAdd = counterToAdd
        });
    }

    public void IncrementFamilyAchievement(long familyId, int achievementId)
    {
        IncrementFamilyAchievement(familyId, achievementId, 1);
    }

    public void IncrementFamilyMission(long familyId, long? playerId, int missionId, int counterToAdd)
    {
        _pendingMissionMessages.Enqueue(new FamilyMissionIncrementMessage
        {
            FamilyId = familyId,
            CharacterId = playerId,
            MissionId = missionId,
            ValueToAdd = counterToAdd
        });
    }

    public void IncrementFamilyMission(long familyId, long? playerId, int missionId)
    {
        IncrementFamilyMission(familyId, playerId, missionId, 1);
    }

    public void IncrementFamilyMission(long familyId, int missionId)
    {
        IncrementFamilyMission(familyId, null, missionId);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await FlushPendingAchievementIncrements();
            await FlushPendingMissionIncrements();

            await Task.Delay(RefreshDelay, stoppingToken);
        }
    }

    private async Task FlushPendingAchievementIncrements()
    {
        try
        {
            if (_pendingMessages.IsEmpty)
            {
                return;
            }

            while (_pendingMessages.TryDequeue(out FamilyAchievementIncrementMessage msg))
            {
                await _messagePublisher.PublishAsync(msg);
            }
        }
        catch (Exception e)
        {
            Log.Error("[FamilyAchievementManager] ExecuteAsync", e);
        }
    }

    private async Task FlushPendingMissionIncrements()
    {
        try
        {
            if (_pendingMissionMessages.IsEmpty)
            {
                return;
            }

            while (_pendingMissionMessages.TryDequeue(out FamilyMissionIncrementMessage msg))
            {
                await _missionMessagePublisher.PublishAsync(msg);
            }
        }
        catch (Exception e)
        {
            Log.Error("[FamilyAchievementManager] ExecuteAsync", e);
        }
    }
}