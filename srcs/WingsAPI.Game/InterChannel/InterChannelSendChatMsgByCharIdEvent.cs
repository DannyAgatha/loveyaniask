using WingsEmu.Game._i18n;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendChatMsgByCharIdEvent : PlayerEvent
{
    public InterChannelSendChatMsgByCharIdEvent(long characterId, GameDialogKey dialogKey, ChatMessageColorType chatMessageColorType)
    {
        DialogKey = dialogKey;
        ChatMessageColorType = chatMessageColorType;
        CharacterId = characterId;
    }

    public long CharacterId { get; }

    public GameDialogKey DialogKey { get; }

    public ChatMessageColorType ChatMessageColorType { get; }
}