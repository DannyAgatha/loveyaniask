using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mails.Events;

public class NoteCreateEvent : PlayerEvent
{
    public NoteCreateEvent(string receiverName, string title, string message)
    {
        ReceiverName = receiverName;
        Title = title;
        Message = message;
    }

    public string ReceiverName { get; }

    public string Title { get; }

    public string Message { get; }
}