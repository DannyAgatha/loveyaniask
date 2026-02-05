using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class NoteOpenEvent : PlayerEvent
{
    public NoteOpenEvent(long noteId, bool isSenderCopy)
    {
        NoteId = noteId;
        IsSenderCopy = isSenderCopy;
    }

    public long NoteId { get; }
    public bool IsSenderCopy { get; }
}