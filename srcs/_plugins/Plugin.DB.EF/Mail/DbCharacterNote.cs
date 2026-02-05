using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using Plugin.Database.Entities.PlayersData;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.Database.Mail
{
    [Table("characters_notes", Schema = DatabaseSchemas.MAILS)]
    public class DbCharacterNote : BaseAuditableEntity, ILongEntity
    {
        public DateTime Date { get; set; }

        public long SenderId { get; set; }

        public long ReceiverId { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Message { get; set; }

        [MaxLength(255)]
        public string EquipmentPackets { get; set; }

        public bool IsSenderCopy { get; set; }

        public bool IsOpened { get; set; }

        public GenderType SenderGender { get; set; }

        public ClassType SenderClass { get; set; }

        public HairColorType SenderHairColor { get; set; }

        public HairStyleType SenderHairStyle { get; set; }

        public string SenderName { get; set; }

        public string ReceiverName { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual DbCharacter Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual DbCharacter Receiver { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}