using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;
using WingsEmu.Plugins.DistributedGameEvents.Relation;

namespace RelationServer.Consumer
{
    public class RelationCharacterDisconnectMessageConsumer : IMessageConsumer<PlayerDisconnectedChannelMessage>
    {
        private readonly IMessagePublisher<RelationCharacterLeaveMessage> _messagePublisher;

        public RelationCharacterDisconnectMessageConsumer(IMessagePublisher<RelationCharacterLeaveMessage> messagePublisher) => _messagePublisher = messagePublisher;

        public async Task HandleAsync(PlayerDisconnectedChannelMessage notification, CancellationToken token)
        {
            await _messagePublisher.PublishAsync(new RelationCharacterLeaveMessage
            {
                CharacterId = notification.CharacterId,
                CharacterName = notification.CharacterName
            });
        }
    }
}