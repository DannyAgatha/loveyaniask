using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace FamilyServer.Consumers
{
    public class FamilyCharacterConnectMessageConsumer : IMessageConsumer<PlayerConnectedOnChannelMessage>
    {
        private readonly IMessagePublisher<FamilyCharacterJoinMessage> _messagePublisher;

        public FamilyCharacterConnectMessageConsumer(IMessagePublisher<FamilyCharacterJoinMessage> messagePublisher) => _messagePublisher = messagePublisher;

        public async Task HandleAsync(PlayerConnectedOnChannelMessage notification, CancellationToken token)
        {
            await _messagePublisher.PublishAsync(new FamilyCharacterJoinMessage
            {
                CharacterId = notification.CharacterId,
                FamilyId = notification.FamilyId
            });
        }
    }
}