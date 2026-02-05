using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.InterChannel;

public class InterChannelChatMessageBroadcastEvent : IAsyncEvent
{
    public InterChannelChatMessageBroadcastEvent(GameDialogKey dialogKey, ChatMessageColorType chatMessageColorType, params string[] args)
    {
        DialogKey = dialogKey;
        ChatMessageColorType = chatMessageColorType;
        Args = args;
    }

    public GameDialogKey DialogKey { get; }

    public ChatMessageColorType ChatMessageColorType { get; }

    public object?[] Args { get; }
}