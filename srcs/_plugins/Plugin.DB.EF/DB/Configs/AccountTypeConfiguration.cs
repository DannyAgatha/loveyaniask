using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.Account;

namespace Plugin.Database.DB.Configs
{
    public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountEntity>
    {
        public void Configure(EntityTypeBuilder<AccountEntity> builder)
        {
            builder
                .Property(e => e.Password)
                .IsUnicode(false);

            builder
                .HasMany(e => e.Character)
                .WithOne(e => e.AccountEntity)
                .HasForeignKey(s => s.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.AccountBans)
                .WithOne(e => e.AccountEntity)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.AccountPenalties)
                .WithOne(e => e.AccountEntity)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(e => e.WarehouseItems)
                .WithOne(e => e.Account)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}