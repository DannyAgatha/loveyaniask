using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.sendbyname.info")]
    public class InterChannelSendInfoByNicknameMessage : IMessage
    {
        public string Nickname { get; set; }

        public GameDialogKey DialogKey { get; set; }
    }
}