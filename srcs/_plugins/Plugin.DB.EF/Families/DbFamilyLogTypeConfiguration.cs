using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Families
{
    public class DbFamilyLogTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbFamilyLog>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbFamilyLog> builder)
        {
            builder.HasKey(s => s.Id);

            builder
                .HasOne(s => s.Family)
                .WithMany(s => s.FamilyLogs)
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}