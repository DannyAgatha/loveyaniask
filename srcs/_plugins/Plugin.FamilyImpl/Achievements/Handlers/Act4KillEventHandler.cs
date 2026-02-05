using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Families;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.FamilyImpl.Achievements.Handlers
{
    public class Act4KillEventHandler : IAsyncEventProcessor<Act4KillEvent>
    {
        private readonly IFamilyMissionManager _familyManager;

        public Act4KillEventHandler(IFamilyMissionManager familyManager) => _familyManager = familyManager;

        public async Task HandleAsync(Act4KillEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            _familyManager.IncrementFamilyMission(session.PlayerEntity.Family.Id, (short)FamilyMissionVnums.DAILY_DEFEAT_10_ENEMIES_ACT4);
        }
    }
}