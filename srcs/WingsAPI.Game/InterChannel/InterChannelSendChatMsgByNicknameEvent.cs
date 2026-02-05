using WingsEmu.Game._i18n;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendChatMsgByNicknameEvent : PlayerEvent
{
    public InterChannelSendChatMsgByNicknameEvent(string nickname, GameDialogKey dialogKey, ChatMessageColorType chatMessageColorType)
    {
        DialogKey = dialogKey;
        ChatMessageColorType = chatMessageColorType;
        Nickname = nickname;
    }

    public string Nickname { get; }

    public GameDialogKey DialogKey { get; }

    public ChatMessageColorType ChatMessageColorType { get; }
}