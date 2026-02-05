// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Achievements;

namespace FamilyServer.Achievements
{
    public class FamilyAchievementIncrementMessageConsumer : IMessageConsumer<FamilyAchievementIncrementMessage>
    {
        private readonly FamilyAchievementManager _familyAchievementManager;

        public FamilyAchievementIncrementMessageConsumer(FamilyAchievementManager familyAchievementManager) => _familyAchievementManager = familyAchievementManager;

        public Task HandleAsync(FamilyAchievementIncrementMessage notification, CancellationToken token)
        {
            var incrementRequest = new FamilyAchievementIncrement { AchievementId = notification.AchievementId, FamilyId = notification.FamilyId, ValueToAdd = notification.ValueToAdd };
            _familyAchievementManager.AddIncrementToQueue(incrementRequest);
            return Task.CompletedTask;
        }
    }
}