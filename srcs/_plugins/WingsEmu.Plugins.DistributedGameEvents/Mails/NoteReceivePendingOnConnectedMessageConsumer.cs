using System.Collections.Generic;
using System.Linq;
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

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    public class NoteReceivePendingOnConnectedMessageConsumer : IMessageConsumer<NoteReceivePendingOnConnectedMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public NoteReceivePendingOnConnectedMessageConsumer(IGameLanguageService gameLanguage, ISessionManager sessionManager)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(NoteReceivePendingOnConnectedMessage e, CancellationToken cancellation)
        {
            long characterId = e.CharacterId;
            List<CharacterNoteDto> notes = e.Notes;

            IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);
            if (session == null)
            {
                return;
            }

            foreach (CharacterNoteDto note in notes)
            {
                byte slot = session.GetNextNoteSlot(note.IsSenderCopy);
                var newNote = new CharacterNote(note, slot);

                session.PlayerEntity.MailNoteComponent.AddNote(newNote);
                session.SendMailPacket(newNote);
            }

            int notOpenedNotes = notes.Count(x => !x.IsOpened && !x.IsSenderCopy);
            if (notOpenedNotes == 0)
            {
                return;
            }
            
            session.SendSayi(ChatMessageColorType.Green, Game18NConstString.NewNoteArrived,4, notOpenedNotes);
        }
    }
}