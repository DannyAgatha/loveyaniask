using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;

namespace FamilyServer.Achievements
{
    public class FamilyAchievementIncrementEventHandler : IAsyncEventProcessor<FamilyAchievementIncrement>
    {
        private readonly FamilyAchievementManager _familyAchievementManager;

        public FamilyAchievementIncrementEventHandler(FamilyAchievementManager familyAchievementManager) => _familyAchievementManager = familyAchievementManager;

        public Task HandleAsync(FamilyAchievementIncrement e, CancellationToken cancellation)
        {
            _familyAchievementManager.AddIncrementToQueue(e);
            return Task.CompletedTask;
        }
    }
}