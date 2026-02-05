using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.Account;

namespace Plugin.Database.DB.Configs
{
    public class AccountBansTypeConfiguration : IEntityTypeConfiguration<AccountBanEntity>
    {
        public void Configure(EntityTypeBuilder<AccountBanEntity> builder)
        {
            builder
                .HasOne(e => e.AccountEntity)
                .WithMany(e => e.AccountBans)
                .HasForeignKey(s => s.AccountId);
        }
    }
}