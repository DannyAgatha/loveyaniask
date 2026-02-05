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
using WingsEmu.DTOs.Mails;

namespace Plugin.Database.Mail
{
    [Table("characters_mails", Schema = DatabaseSchemas.MAILS)]
    public class DbCharacterMail : BaseAuditableEntity, ILongEntity
    {
        public DateTime Date { get; set; }

        [MaxLength(255)]
        public string SenderName { get; set; }

        public long ReceiverId { get; set; }

        public MailGiftType MailGiftType { get; set; }

        [Column(TypeName = "jsonb")]
        public ItemInstanceDTO ItemInstance { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual DbCharacter Receiver { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}