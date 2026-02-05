using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Bazaar;

namespace Plugin.Database.DB.Configs
{
    public class CharacterBazaarItemEntityTypeConfiguration : IEntityTypeConfiguration<DbBazaarItemEntity>
    {
        public void Configure(EntityTypeBuilder<DbBazaarItemEntity> builder)
        {
            builder
                .HasOne(s => s.DbCharacter)
                .WithMany(s => s.BazaarItem)
                .HasForeignKey(s => s.CharacterId);
        }
    }
}