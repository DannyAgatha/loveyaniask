using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Families
{
    public class DbFamilyTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbFamily>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbFamily> builder)
        {
            builder.HasKey(s => s.Id);

            builder
                .Property(s => s.Name)
                .HasMaxLength(30)
                .IsRequired()
                .IsUnicode();


            builder
                .HasMany(s => s.FamilyCharacters)
                .WithOne(s => s.Family)
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(s => s.FamilyLogs)
                .WithOne(s => s.Family)
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(s => s.WarehouseItems)
                .WithOne(s => s.Family)
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}