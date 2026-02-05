using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.FamilyImpl.Messages
{
    [MessageType("family.character.join")]
    public class FamilyCharacterJoinMessage : IMessage
    {
        public long CharacterId { get; set; }
        public long? FamilyId { get; set; }
    }
}