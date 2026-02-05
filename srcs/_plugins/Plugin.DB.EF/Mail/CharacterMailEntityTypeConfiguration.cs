using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Mail
{
    public class CharacterMailEntityTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbCharacterMail>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbCharacterMail> builder)
        {
            builder
                .HasOne(s => s.Receiver)
                .WithMany(s => s.ReceivedMails)
                .HasForeignKey(s => s.ReceiverId);
        }
    }
}