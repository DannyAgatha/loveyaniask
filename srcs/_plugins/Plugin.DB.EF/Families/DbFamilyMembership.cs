// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using Plugin.Database.Entities.PlayersData;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.Database.Families
{
    [Table("families_memberships", Schema = DatabaseSchemas.FAMILIES)]
    public class DbFamilyMembership : BaseAuditableEntity, ILongEntity
    {
        public long? CharacterId { get; set; }
        public long FamilyId { get; set; }

        public FamilyAuthority Authority { get; set; }

        [MaxLength(50)]
        public string DailyMessage { get; set; }

        public long Experience { get; set; }

        public FamilyTitle Title { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime LastOnlineDate { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public virtual DbCharacter DbCharacter { get; set; }

        [ForeignKey(nameof(FamilyId))]
        public virtual DbFamily Family { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}