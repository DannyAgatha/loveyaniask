using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Warehouse
{
    public class AccountWarehouseItemEntityTypeConfiguration : BaseAuditableEntityTypeConfiguration<AccountWarehouseItemEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<AccountWarehouseItemEntity> builder)
        {
            builder.HasKey(x => new { x.AccountId, x.Slot });

            builder
                .HasOne(s => s.Account)
                .WithMany(s => s.WarehouseItems)
                .HasForeignKey(s => s.AccountId);
        }
    }
}