using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.PlayersData;

namespace Plugin.Database.DB.Configs
{
    public class CharacterRelationEntityTypeConfiguration : IEntityTypeConfiguration<CharacterRelationEntity>
    {
        public void Configure(EntityTypeBuilder<CharacterRelationEntity> builder)
        {
            builder.HasKey(s => new { s.CharacterId, s.RelatedCharacterId });

            builder.HasOne(s => s.Source)
                .WithMany(s => s.SourceRelations)
                .HasForeignKey(s => s.CharacterId);

            builder.HasOne(s => s.Target)
                .WithMany(s => s.TargetRelations)
                .HasForeignKey(s => s.RelatedCharacterId);
        }
    }
}