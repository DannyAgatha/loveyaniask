namespace WingsEmu.Game.Chat;

public class GlobalPlayerChatReceivedEvent : ChatMessageReceivedEvent
{
    public GlobalPlayerChatReceivedEvent(string senderName, string senderMessage, long senderChannelId)
        : base(senderName, senderMessage, senderChannelId)
    {
    }
}