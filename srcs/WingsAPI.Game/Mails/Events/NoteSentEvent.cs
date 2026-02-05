using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class NoteSentEvent : PlayerEvent
{
    public long NoteId { get; init; }
    public string ReceiverName { get; init; }
    public string Title { get; set; }
    public string Message { get; init; }
}