using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Account;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("interchannel.sendbyname.whisper")]
    public class InterChannelSendWhisperMessage : IMessage
    {
        public long SenderCharacterId { get; set; }
        public string SenderNickname { get; set; }
        public int SenderChannelId { get; set; }

        public AuthorityType AuthorityType { get; set; }

        public string ReceiverNickname { get; set; }

        public string Message { get; set; }
    }
}