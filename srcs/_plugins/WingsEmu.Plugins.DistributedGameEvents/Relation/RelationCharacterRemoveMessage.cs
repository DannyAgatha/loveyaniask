using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    [MessageType("relation.character.remove")]
    public class RelationCharacterRemoveMessage : IMessage
    {
        public long CharacterId { get; set; }
        public long TargetId { get; set; }
        public CharacterRelationType RelationType { get; set; }
    }
}