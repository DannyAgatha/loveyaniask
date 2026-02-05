using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    [MessageType("relation.character.add")]
    public class RelationCharacterAddMessage : IMessage
    {
        public CharacterRelationDTO SenderRelation { get; set; }
        public CharacterRelationDTO TargetRelation { get; set; }
    }
}