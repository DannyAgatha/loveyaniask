using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Achievements;

namespace FamilyServer.Achievements
{
    public class FamilyMissionIncrementMessageConsumer : IMessageConsumer<FamilyMissionIncrementMessage>
    {
        private readonly FamilyAchievementManager _familyAchievementManager;

        public FamilyMissionIncrementMessageConsumer(FamilyAchievementManager familyAchievementManager) => _familyAchievementManager = familyAchievementManager;

        public Task HandleAsync(FamilyMissionIncrementMessage notification, CancellationToken token)
        {
            var incrementRequest = new FamilyMissionIncrement
            {
                MissionId = notification.MissionId,
                CharacterId = notification.CharacterId,
                FamilyId = notification.FamilyId,
                ValueToAdd = notification.ValueToAdd
            };
            _familyAchievementManager.AddIncrementToQueue(incrementRequest);
            return Task.CompletedTask;
        }
    }
}