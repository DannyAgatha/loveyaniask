using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Chat;

namespace WingsEmu.Game.InterChannel
{
    public class InterchannelChatSpeakerEvent : PlayerEvent
    {
        public ChatSpeakerEvent Event { get; init; }
        public int ChannelId { get; init; }
    }
}