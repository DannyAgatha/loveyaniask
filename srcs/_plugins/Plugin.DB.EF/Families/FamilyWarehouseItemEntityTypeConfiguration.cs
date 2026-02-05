using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Families
{
    public class FamilyWarehouseItemEntityTypeConfiguration : BaseAuditableEntityTypeConfiguration<FamilyWarehouseItemEntity>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<FamilyWarehouseItemEntity> builder)
        {
            builder.HasKey(s => new { s.FamilyId, s.Slot });

            builder.HasOne(s => s.Family)
                .WithMany(s => s.WarehouseItems)
                .HasForeignKey(s => s.FamilyId);
        }
    }
}