using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyMemberInviteMessageConsumer : IMessageConsumer<FamilyInviteMessage>
    {
        private readonly ISessionManager _sessionManager;

        public FamilyMemberInviteMessageConsumer(ISessionManager sessionManager) => _sessionManager = sessionManager;

        public async Task HandleAsync(FamilyInviteMessage e, CancellationToken cancellation)
        {
            IClientSession localSession = _sessionManager.GetSessionByCharacterName(e.ReceiverNickname);

            if (localSession == null)
            {
                return;
            }

            if (localSession.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
            {
                return;
            }
            
            if (localSession.PlayerEntity.MapInstance.IsAct6PvpInstance)
            {
                return;
            }

            await localSession.EmitEventAsync(new FamilyReceiveInviteEvent(e.FamilyName, e.SenderCharacterId, e.FamilyId));
        }
    }
}