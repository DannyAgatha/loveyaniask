using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.character.leave")]
    public class FamilyCharacterLeaveMessage : IMessage
    {
        public long FamilyId { get; set; }
        public long CharacterId { get; set; }
    }
}