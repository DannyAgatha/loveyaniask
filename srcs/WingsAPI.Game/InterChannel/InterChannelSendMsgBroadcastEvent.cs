using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendMsgBroadcastEvent : IAsyncEvent
{
    public GameDialogKey DialogKey { get; set; }
    public MsgMessageType MessageType { get; set; }
    public string[]? MessageArgs { get; set; }
}