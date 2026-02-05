using System.Collections.Generic;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;
using WingsEmu.DTOs.Relations;

namespace WingsEmu.Plugins.DistributedGameEvents.Relation
{
    [MessageType("relation.character.join")]
    public class RelationCharacterJoinMessage : IMessage
    {
        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public List<CharacterRelationDTO> Relations { get; set; }
    }
}