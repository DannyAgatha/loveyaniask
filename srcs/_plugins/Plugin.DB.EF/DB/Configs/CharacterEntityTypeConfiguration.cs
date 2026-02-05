// NosEmu
// 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.PlayersData;

namespace Plugin.Database.DB.Configs
{
    public class CharacterEntityTypeConfiguration : BaseAuditableEntityTypeConfiguration<DbCharacter>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<DbCharacter> builder)
        {
            builder
                .Property(e => e.Name)
                .IsUnicode(false);


            builder
                .HasMany(e => e.BazaarItem)
                .WithOne(e => e.DbCharacter)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.SourceRelations)
                .WithOne(e => e.Source)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.TargetRelations)
                .WithOne(e => e.Target)
                .HasForeignKey(e => e.RelatedCharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.ReceivedMails)
                .WithOne(e => e.Receiver)
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.SentNotes)
                .WithOne(e => e.Sender)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.ReceivedNotes)
                .WithOne(e => e.Receiver)
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(s => s.FamilyCharacter)
                .WithOne(s => s.DbCharacter)
                .HasForeignKey(s => s.CharacterId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}