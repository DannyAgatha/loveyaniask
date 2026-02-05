using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.broadcast.chatmessage")]
    public class InterChannelChatMessageBroadcastMessage : IMessage
    {
        public GameDialogKey DialogKey { get; set; }

        public ChatMessageColorType ChatMessageColorType { get; set; }

        public object?[] Args { get; set; }
    }
}