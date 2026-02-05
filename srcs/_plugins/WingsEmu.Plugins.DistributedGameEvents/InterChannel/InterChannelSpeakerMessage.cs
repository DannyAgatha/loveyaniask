using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.speaker")]
    public class InterChannelSpeakerMessage : IMessage
    {
        public string Message { get; init; }
        public int UserId { get; init; }
        public int ChannelId { get; init; }
    }
}