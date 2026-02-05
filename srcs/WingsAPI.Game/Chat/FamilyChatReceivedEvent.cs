namespace WingsEmu.Game.Chat;

public class FamilyChatReceivedEvent : ChatMessageReceivedEvent
{
    public FamilyChatReceivedEvent(string senderName, string senderMessage, long senderChannelId, long targetFamilyId)
        : base(senderName, senderMessage, senderChannelId) =>
        TargetFamilyId = targetFamilyId;

    public long TargetFamilyId { get; }
}