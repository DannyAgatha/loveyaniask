using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyNoticeMessageEvent : PlayerEvent
{
    public FamilyNoticeMessageEvent(string message, bool cleanMessage = false)
    {
        Message = message;
        CleanMessage = cleanMessage;
    }

    public string Message { get; }
    public bool CleanMessage { get; }
}