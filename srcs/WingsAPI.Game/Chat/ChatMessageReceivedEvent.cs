// NosEmu
// 


using PhoenixLib.Events;

namespace WingsEmu.Game.Chat;

public abstract class ChatMessageReceivedEvent : IAsyncEvent
{
    protected ChatMessageReceivedEvent(string senderName, string senderMessage, long senderChannelId)
    {
        SenderName = senderName;
        SenderMessage = senderMessage;
        SenderChannelId = senderChannelId;
    }

    public string SenderName { get; }
    public string SenderMessage { get; }
    public long SenderChannelId { get; }
}