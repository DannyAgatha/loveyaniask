using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.chatmessage")]
    public class FamilyChatMessage : IMessage
    {
        public long SenderFamilyId { get; set; }

        public int SenderChannelId { get; set; }

        public string SenderNickname { get; set; }

        public string Message { get; set; }
    }
}