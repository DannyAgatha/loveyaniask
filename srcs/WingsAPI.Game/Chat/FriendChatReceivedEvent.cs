namespace WingsEmu.Game.Chat;

public class FriendChatReceivedEvent : ChatMessageReceivedEvent
{
    public FriendChatReceivedEvent(string senderName, string senderMessage, long senderChannelId, long targetCharacterId)
        : base(senderName, senderMessage, senderChannelId) =>
        TargetCharacterId = targetCharacterId;

    public long TargetCharacterId { get; }
}