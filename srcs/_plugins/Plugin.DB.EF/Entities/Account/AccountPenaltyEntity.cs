using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using WingsEmu.Packets.Enums;

namespace Plugin.Database.Entities.Account
{
    [Table("accounts_penalties", Schema = DatabaseSchemas.ACCOUNTS)]
    public class AccountPenaltyEntity : ILongEntity
    {
        public long AccountId { get; set; }

        public string JudgeName { get; set; }

        public string TargetName { get; set; }

        public DateTime Start { get; set; }

        public int? RemainingTime { get; set; }

        public PenaltyType PenaltyType { get; set; }

        public string Reason { get; set; }

        public string UnlockReason { get; set; }

        public virtual AccountEntity AccountEntity { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}