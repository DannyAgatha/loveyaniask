using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.sendbycharid.chatmsg")]
    public class InterChannelSendChatMsgByCharIdMessage : IMessage
    {
        public long CharacterId { get; set; }

        public GameDialogKey DialogKey { get; set; }

        public ChatMessageColorType ChatMessageColorType { get; set; }
    }
}