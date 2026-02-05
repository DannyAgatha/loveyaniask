using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Chat;

public class ChatSendFriendMessageEvent : PlayerEvent
{
    public long TargetId { get; set; }
    public string Message { get; set; }
}