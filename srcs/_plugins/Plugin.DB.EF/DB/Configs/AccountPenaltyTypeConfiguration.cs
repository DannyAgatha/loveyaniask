using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.Account;

namespace Plugin.Database.DB.Configs
{
    public class AccountPenaltyTypeConfiguration : IEntityTypeConfiguration<AccountPenaltyEntity>
    {
        public void Configure(EntityTypeBuilder<AccountPenaltyEntity> builder)
        {
            builder
                .HasOne(e => e.AccountEntity)
                .WithMany(e => e.AccountPenalties)
                .HasForeignKey(s => s.AccountId);
        }
    }
}