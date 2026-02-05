using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel;

[MessageType("interchannel.msg.broadcast.server")]
public class InterChannelSendMsgBroadcastMessage : IMessage
{
    public GameDialogKey DialogKey { get; set; }
    public MsgMessageType MessageType { get; set; }
    public object[] MessageArgs { get; set; }
} 