using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.DTOs.Mails;
using WingsEmu.Plugins.DistributedGameEvents.Mails;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace MailServer.Consumers
{
    public class NoteCharacterConnectedMessageConsumer : IMessageConsumer<PlayerConnectedOnChannelMessage>
    {
        private readonly ICharacterNoteDao _characterNoteDao;
        private readonly IMessagePublisher<NoteReceivePendingOnConnectedMessage> _publisher;

        public NoteCharacterConnectedMessageConsumer(IMessagePublisher<NoteReceivePendingOnConnectedMessage> publisher, ICharacterNoteDao characterNoteDao)
        {
            _publisher = publisher;
            _characterNoteDao = characterNoteDao;
        }

        public async Task HandleAsync(PlayerConnectedOnChannelMessage e, CancellationToken cancellation)
        {
            long characterId = e.CharacterId;
            List<CharacterNoteDto> notes = await _characterNoteDao.GetByCharacterIdAsync(characterId);

            await _publisher.PublishAsync(new NoteReceivePendingOnConnectedMessage
            {
                CharacterId = characterId,
                Notes = notes
            });
        }
    }
}