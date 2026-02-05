using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using WingsEmu.DTOs.Items;

namespace Plugin.Database.Families
{
    [Table("families_warehouses", Schema = DatabaseSchemas.FAMILIES)]
    public class FamilyWarehouseItemEntity : BaseAuditableEntity
    {
        public long FamilyId { get; set; }

        public short Slot { get; set; }

        [Column(TypeName = "jsonb")]
        public ItemInstanceDTO ItemInstance { get; set; }

        [ForeignKey(nameof(FamilyId))]
        public virtual DbFamily Family { get; set; }
    }
}