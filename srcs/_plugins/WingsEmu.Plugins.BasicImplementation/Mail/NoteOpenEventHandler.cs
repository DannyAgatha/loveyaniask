using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.Mail;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Mail;

public class NoteOpenEventHandler : IAsyncEventProcessor<NoteOpenEvent>
{
    private readonly INoteService _noteService;

    public NoteOpenEventHandler(INoteService noteService) => _noteService = noteService;

    public async Task HandleAsync(NoteOpenEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long noteId = e.NoteId;
        bool isSenderCopy = e.IsSenderCopy;

        CharacterNote note = session.PlayerEntity.MailNoteComponent.GetNote(noteId, isSenderCopy);
        if (note == null)
        {
            return;
        }

        if (!note.IsOpened && !note.IsSenderCopy)
        {
            BasicRpcResponse response = await _noteService.OpenNoteAsync(new OpenNoteRequest
            {
                NoteId = note.Id
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return;
            }

            note.IsOpened = true;
        }

        session.SendPostMessage(note, (byte)(note.IsSenderCopy ? 2 : 1));
    }
}