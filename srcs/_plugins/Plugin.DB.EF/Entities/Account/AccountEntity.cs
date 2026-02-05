// NosEmu
// 


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Warehouse;
using ProtoBuf;
using WingsEmu.DTOs.Account;

namespace Plugin.Database.Entities.Account
{
    [Table("accounts", Schema = DatabaseSchemas.ACCOUNTS)]
    public class AccountEntity : BaseAuditableEntity, ILongEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid MasterAccountId { get; set; }

        public AuthorityType Authority { get; set; }

        public AccountLanguage Language { get; set; }

        public long BankMoney { get; set; }

        public bool IsPrimaryAccount { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(45)]
        public string RegistrationIP { get; set; }

        public virtual ICollection<DbCharacter> Character { get; set; }
        public virtual ICollection<AccountBanEntity> AccountBans { get; set; }
        public virtual ICollection<AccountPenaltyEntity> AccountPenalties { get; set; }
        public virtual IEnumerable<AccountWarehouseItemEntity> WarehouseItems { get; set; }
        
        public virtual ICollection<RefreshTokensEntity> RefreshTokens { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}