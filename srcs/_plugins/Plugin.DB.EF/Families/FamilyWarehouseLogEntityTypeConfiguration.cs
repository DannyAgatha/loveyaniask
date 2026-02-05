using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Plugin.Database.Families
{
    public class FamilyWarehouseLogEntityTypeConfiguration : IEntityTypeConfiguration<FamilyWarehouseLogEntity>
    {
        public void Configure(EntityTypeBuilder<FamilyWarehouseLogEntity> builder)
        {
            builder.HasKey(s => new { s.FamilyId });

            builder.HasOne(s => s.Family)
                .WithOne(s => s.WarehouseLogs);
        }
    }
}