// NosEmu
// 


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Plugin.Database.Auth.ClientVersion;
using Plugin.Database.Auth.HWID;
using Plugin.Database.Bazaar;
using Plugin.Database.DB.Configs;
using Plugin.Database.Entities;
using Plugin.Database.Entities.Account;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Entities.ServerData;
using Plugin.Database.Families;
using Plugin.Database.Mail;
using Plugin.Database.Warehouse;

namespace Plugin.Database.DB
{
    public class GameContext : DbContext
    {
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        public DbSet<BlacklistedHwidEntity> BlacklistedHwids { get; set; }
        public DbSet<AuthorizedClientVersionEntity> AuthorizedClientVersions { get; set; }


        public DbSet<AccountEntity> Account { get; set; }
        public DbSet<AccountWarehouseItemEntity> AccountWarehouseItems { get; set; }
        public DbSet<AccountBanEntity> AccountBans { get; set; }
        public DbSet<AccountPenaltyEntity> AccountPenalties { get; set; }

        #region Bazaar

        public DbSet<DbBazaarItemEntity> BazaarItem { get; set; }

        #endregion


        public DbSet<DbTimeSpaceRecord> TimeSpaceRecords { get; set; }
        
        public DbSet<RefreshTokensEntity> RefreshTokens { get; set; }

        public override int SaveChanges()
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatuses();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
        {
            UpdateSoftDeleteStatuses();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        private void UpdateSoftDeleteStatuses()
        {
            foreach (EntityEntry entry in ChangeTracker.Entries().Where(s => s.Entity is IAuditableEntity))
            {
                var entity = (IAuditableEntity)entry.Entity;
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // accounts
            modelBuilder.ApplyConfiguration(new AccountTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AccountPenaltyTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AccountBansTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AccountWarehouseItemEntityTypeConfiguration());

            // player data
            modelBuilder.ApplyConfiguration(new CharacterEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterRelationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterBazaarItemEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DbTimeSpaceRecordTypeConfiguration());


            // families data
            modelBuilder.ApplyConfiguration(new DbFamilyTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DbFamilyCharacterTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DbFamilyLogTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FamilyWarehouseItemEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new FamilyWarehouseLogEntityTypeConfiguration());

            // mails
            modelBuilder.ApplyConfiguration(new CharacterMailEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DbCharacterNoteEntityTypeConfiguration());
        }

        #region Character

        public DbSet<DbCharacter> Character { get; set; }
        public DbSet<CharacterRelationEntity> CharacterRelation { get; set; }
        public DbSet<DbCharacterMail> Mail { get; set; }
        public DbSet<DbCharacterNote> Note { get; set; }

        #endregion

        #region Family

        public DbSet<DbFamily> Family { get; set; }
        public DbSet<DbFamilyMembership> FamilyCharacter { get; set; }
        public DbSet<DbFamilyLog> FamilyLog { get; set; }
        public DbSet<FamilyWarehouseItemEntity> FamilyWarehouseItems { get; set; }
        public DbSet<FamilyWarehouseLogEntity> FamilyWarehouseLogs { get; set; }

        #endregion
    }
}