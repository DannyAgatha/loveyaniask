// NosEmu
// 


using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using Plugin.Database.Entities.PlayersData;
using WingsEmu.DTOs.Items;

namespace Plugin.Database.Bazaar
{
    [Table("items", Schema = DatabaseSchemas.BAZAAR)]
    public class DbBazaarItemEntity : BaseAuditableEntity, ILongEntity
    {
        public long CharacterId { get; set; }


        public int Amount { get; set; }

        public int SoldAmount { get; set; }

        public long PricePerItem { get; set; }

        public long SaleFee { get; set; }

        public bool IsPackage { get; set; }

        public bool UsedMedal { get; set; }

        public DateTime ExpiryDate { get; set; }

        public short DayExpiryAmount { get; set; }

        [Column(TypeName = "jsonb")]
        public ItemInstanceDTO ItemInstance { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public virtual DbCharacter DbCharacter { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}