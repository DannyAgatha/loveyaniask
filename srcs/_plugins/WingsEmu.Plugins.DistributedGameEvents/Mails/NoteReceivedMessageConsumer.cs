using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace WingsEmu.Plugins.DistributedGameEvents.Consumer
{
    public class NoteReceivedMessageConsumer : IMessageConsumer<NoteReceivedMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public NoteReceivedMessageConsumer(IGameLanguageService gameLanguage, ISessionManager sessionManager)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(NoteReceivedMessage e, CancellationToken cancellation)
        {
            CharacterNoteDto senderNote = e.SenderNote;
            CharacterNoteDto receiverNote = e.ReceiverNote;

            SenderNote(senderNote);

            IClientSession receiver = _sessionManager.GetSessionByCharacterId(receiverNote.ReceiverId);
            if (receiver == null)
            {
                return;
            }

            var newReceiverNote = new CharacterNote(receiverNote, receiver.GetNextNoteSlot(receiverNote.IsSenderCopy));
            receiver.PlayerEntity.MailNoteComponent.AddNote(newReceiverNote);
            receiver.SendMailPacket(newReceiverNote);
            receiver.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.NewNoteArrived2);
        }

        private void SenderNote(CharacterNoteDto senderNote)
        {
            if (!senderNote.IsSenderCopy)
            {
                return;
            }

            IClientSession session = _sessionManager.GetSessionByCharacterId(senderNote.SenderId);
            if (session == null)
            {
                return;
            }

            var newSenderNote = new CharacterNote(senderNote, session.GetNextNoteSlot(senderNote.IsSenderCopy));
            session.PlayerEntity.MailNoteComponent.AddNote(newSenderNote);
            session.SendMailPacket(newSenderNote);
        }
    }
}