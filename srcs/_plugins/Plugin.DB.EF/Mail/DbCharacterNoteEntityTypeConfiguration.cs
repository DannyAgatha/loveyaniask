// NosEmu
// 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.DB.Configs;

namespace Plugin.Database.Mail
{
    public class DbCharacterNoteEntityTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbCharacterNote>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbCharacterNote> builder)
        {
            builder.HasOne(s => s.Receiver)
                .WithMany(s => s.ReceivedNotes)
                .HasForeignKey(s => s.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}