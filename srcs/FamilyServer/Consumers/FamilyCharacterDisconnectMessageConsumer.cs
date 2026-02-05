using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication.Families;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace FamilyServer.Consumers
{
    public class FamilyCharacterDisconnectMessageConsumer : IMessageConsumer<PlayerDisconnectedChannelMessage>
    {
        private readonly IFamilyService _familyService;
        private readonly IMessagePublisher<FamilyCharacterLeaveMessage> _messagePublisher;

        public FamilyCharacterDisconnectMessageConsumer(IMessagePublisher<FamilyCharacterLeaveMessage> messagePublisher, IFamilyService familyService)
        {
            _messagePublisher = messagePublisher;
            _familyService = familyService;
        }

        public async Task HandleAsync(PlayerDisconnectedChannelMessage e, CancellationToken cancellation)
        {
            if (!e.FamilyId.HasValue)
            {
                return;
            }

            await _familyService.MemberDisconnectedAsync(new FamilyMemberDisconnectedRequest
            {
                CharacterId = e.CharacterId,
                DisconnectionTime = e.DisconnectionTime
            });
            ;
            await _messagePublisher.PublishAsync(new FamilyCharacterLeaveMessage
            {
                CharacterId = e.CharacterId,
                FamilyId = e.FamilyId.Value
            }, cancellation);
        }
    }
}