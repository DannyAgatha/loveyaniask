using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game._i18n;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.sendbycharid.info")]
    public class InterChannelSendInfoByCharIdMessage : IMessage
    {
        public long CharacterId { get; set; }

        public GameDialogKey DialogKey { get; set; }
    }
}