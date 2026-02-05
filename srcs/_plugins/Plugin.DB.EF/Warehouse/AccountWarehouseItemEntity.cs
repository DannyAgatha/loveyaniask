using System.ComponentModel.DataAnnotations.Schema;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using Plugin.Database.Entities.Account;
using WingsEmu.DTOs.Items;

namespace Plugin.Database.Warehouse
{
    [Table("accounts_warehouse", Schema = DatabaseSchemas.ACCOUNTS)]
    public class AccountWarehouseItemEntity : BaseAuditableEntity
    {
        public long AccountId { get; set; }

        public short Slot { get; set; }

        [Column(TypeName = "jsonb")]
        public ItemInstanceDTO ItemInstance { get; set; }

        [ForeignKey(nameof(AccountId))]
        public virtual AccountEntity Account { get; set; }
    }
}