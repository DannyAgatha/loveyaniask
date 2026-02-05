using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    [MessageType("relation.character.leave")]
    public class RelationCharacterLeaveMessage : IMessage
    {
        public long CharacterId { get; set; }
        public string CharacterName { get; set; }
    }
}