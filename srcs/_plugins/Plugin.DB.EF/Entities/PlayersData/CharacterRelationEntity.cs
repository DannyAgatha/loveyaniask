// NosEmu
// 


using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;
using WingsEmu.Packets.Enums.Relations;

namespace Plugin.Database.Entities.PlayersData
{
    [Table("characters_relations", Schema = DatabaseSchemas.CHARACTERS)]
    public class CharacterRelationEntity
    {
        public long CharacterId { get; set; }
        public long RelatedCharacterId { get; set; }

        public string RelatedName { get; set; }

        public CharacterRelationType RelationType { get; set; }

        public virtual DbCharacter Source { get; set; }

        public virtual DbCharacter Target { get; set; }
    }
}