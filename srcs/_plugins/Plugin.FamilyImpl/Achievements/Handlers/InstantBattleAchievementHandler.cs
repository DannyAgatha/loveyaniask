using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Families;
using WingsEmu.Game.GameEvent.InstantBattle;

namespace Plugin.FamilyImpl.Achievements.Handlers
{
    public class InstantBattleAchievementHandler : IAsyncEventProcessor<InstantBattleWonEvent>
    {
        private readonly IFamilyMissionManager _familyMissionManager;

        public InstantBattleAchievementHandler(IFamilyMissionManager familyMissionManager) => _familyMissionManager = familyMissionManager;

        public async Task HandleAsync(InstantBattleWonEvent e, CancellationToken cancellation)
        {
            IPlayerEntity player = e.Sender.PlayerEntity;
            if (!player.IsInFamily())
            {
                return;
            }

            long familyId = player.Family.Id;
            _familyMissionManager.IncrementFamilyMission(familyId, player.Id, (int)FamilyMissionVnums.DAILY_DEFEAT_10_INSTANT_BATTLES);
        }
    }
}