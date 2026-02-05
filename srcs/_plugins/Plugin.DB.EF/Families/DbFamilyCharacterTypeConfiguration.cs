using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Families
{
    public class DbFamilyCharacterTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbFamilyMembership>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbFamilyMembership> builder)
        {
            builder.HasKey(s => s.Id);

            builder
                .HasOne(s => s.Family)
                .WithMany(s => s.FamilyCharacters)
                .HasForeignKey(s => s.FamilyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}