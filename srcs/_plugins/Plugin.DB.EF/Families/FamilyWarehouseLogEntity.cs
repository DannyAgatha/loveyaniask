using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using WingsAPI.Data.Families;

namespace Plugin.Database.Families
{
    [Table("families_warehouses_logs", Schema = DatabaseSchemas.FAMILIES)]
    public class FamilyWarehouseLogEntity : BaseAuditableEntity
    {
        public long FamilyId { get; set; }

        [Column(TypeName = "jsonb")]
        public List<FamilyWarehouseLogEntryDto> LogEntries { get; set; }

        [ForeignKey(nameof(FamilyId))]
        public virtual DbFamily Family { get; set; }
    }
}