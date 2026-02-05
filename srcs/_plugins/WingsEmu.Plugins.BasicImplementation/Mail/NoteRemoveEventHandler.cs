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

public class NoteRemoveEventHandler : IAsyncEventProcessor<NoteRemoveEvent>
{
    private readonly INoteService _noteService;

    public NoteRemoveEventHandler(INoteService noteService) => _noteService = noteService;

    public async Task HandleAsync(NoteRemoveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long noteId = e.NoteId;
        bool isSenderCopy = e.IsSenderCopy;

        CharacterNote note = session.PlayerEntity.MailNoteComponent.GetNote(noteId, isSenderCopy);
        if (note == null)
        {
            return;
        }

        BasicRpcResponse response = await _noteService.RemoveNoteAsync(new RemoveNoteRequest
        {
            NoteId = note.Id
        });

        if (response.ResponseType != RpcResponseType.SUCCESS)
        {
            return;
        }

        session.PlayerEntity.MailNoteComponent.RemoveNote(note);
        session.SendNoteDelete(noteId, note.IsSenderCopy);
    }
}