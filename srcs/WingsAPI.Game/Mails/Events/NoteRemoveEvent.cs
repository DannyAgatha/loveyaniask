using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class NoteRemoveEvent : PlayerEvent
{
    public NoteRemoveEvent(long noteId, bool isSenderCopy)
    {
        NoteId = noteId;
        IsSenderCopy = isSenderCopy;
    }

    public long NoteId { get; }
    public bool IsSenderCopy { get; }
}