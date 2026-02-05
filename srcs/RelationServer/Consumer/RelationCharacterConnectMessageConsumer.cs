using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.DTOs.Relations;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;
using WingsEmu.Plugins.DistributedGameEvents.Relation;

namespace RelationServer.Consumer
{
    public class RelationCharacterConnectMessageConsumer : IMessageConsumer<PlayerConnectedOnChannelMessage>
    {
        private readonly IMessagePublisher<RelationCharacterJoinMessage> _messagePublisher;
        private readonly ICharacterRelationDAO _relationDao;

        public RelationCharacterConnectMessageConsumer(ICharacterRelationDAO relationDao, IMessagePublisher<RelationCharacterJoinMessage> messagePublisher)
        {
            _relationDao = relationDao;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(PlayerConnectedOnChannelMessage notification, CancellationToken token)
        {
            long characterId = notification.CharacterId;
            List<CharacterRelationDTO> relations = await _relationDao.LoadRelationsByCharacterIdAsync(characterId);

            if (!relations.Any())
            {
                return;
            }

            await _messagePublisher.PublishAsync(new RelationCharacterJoinMessage
            {
                CharacterId = characterId,
                CharacterName = notification.CharacterName,
                Relations = relations
            });
        }
    }
}