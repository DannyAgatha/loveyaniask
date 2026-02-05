// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.Database.Families
{
    [Table("families_logs", Schema = DatabaseSchemas.FAMILIES)]
    public class DbFamilyLog : BaseAuditableEntity, ILongEntity
    {
        public long FamilyId { get; set; }

        public FamilyLogType FamilyLogType { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(32)]
        public string Actor { get; set; }

        [MaxLength(16)]
        public string Argument1 { get; set; }

        [MaxLength(16)]
        public string Argument2 { get; set; }

        [MaxLength(16)]
        public string Argument3 { get; set; }

        [ForeignKey(nameof(FamilyId))]
        public virtual DbFamily Family { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}