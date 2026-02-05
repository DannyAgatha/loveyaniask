// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.DTOs.Mails;
using WingsEmu.Plugins.DistributedGameEvents.Mails;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace MailServer.Consumers
{
    public class MailCharacterConnectedMessageConsumer : IMessageConsumer<PlayerConnectedOnChannelMessage>
    {
        private readonly ICharacterMailDao _characterMailDao;
        private readonly IMessagePublisher<MailReceivePendingOnConnectedMessage> _publisher;

        public MailCharacterConnectedMessageConsumer(IMessagePublisher<MailReceivePendingOnConnectedMessage> publisher, ICharacterMailDao characterMailDao)
        {
            _publisher = publisher;
            _characterMailDao = characterMailDao;
        }

        public async Task HandleAsync(PlayerConnectedOnChannelMessage e, CancellationToken cancellation)
        {
            long characterId = e.CharacterId;
            List<CharacterMailDto> mails = await _characterMailDao.GetByCharacterIdAsync(characterId);

            if (!mails.Any())
            {
                return;
            }

            IEnumerable<CharacterMailDto> firstFiftyMails = mails.Take(50);

            var tuple = new List<CharacterMailDto>();
            foreach (CharacterMailDto mail in firstFiftyMails)
            {
                tuple.Add(mail);
            }

            await _publisher.PublishAsync(new MailReceivePendingOnConnectedMessage
            {
                CharacterId = characterId,
                Mails = tuple
            });
        }
    }
}