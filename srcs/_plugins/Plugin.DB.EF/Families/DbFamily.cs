// NosEmu
// 


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.Database.Families
{
    [Table("families", Schema = DatabaseSchemas.FAMILIES)]
    public class DbFamily : BaseAuditableEntity, ILongEntity
    {
        [MinLength(3)]
        [MaxLength(20)]
        public string Name { get; set; }

        public byte Level { get; set; }

        public long Experience { get; set; }

        public byte Faction { get; set; }

        public GenderType HeadGender { get; set; }

        [MaxLength(50)]
        public string Message { get; set; }

        public FamilyWarehouseAuthorityType AssistantWarehouseAuthorityType { get; set; }

        public FamilyWarehouseAuthorityType MemberWarehouseAuthorityType { get; set; }

        public bool AssistantCanGetHistory { get; set; }

        public bool AssistantCanInvite { get; set; }

        public bool AssistantCanNotice { get; set; }

        public bool AssistantCanShout { get; set; }

        public bool MemberCanGetHistory { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyUpgradeDto Upgrades { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyAchievementsDto Achievements { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyMissionsDto Missions { get; set; }

        public virtual ICollection<DbFamilyMembership> FamilyCharacters { get; set; }

        public virtual ICollection<DbFamilyLog> FamilyLogs { get; set; }
        public virtual ICollection<FamilyWarehouseItemEntity> WarehouseItems { get; set; }
        public virtual FamilyWarehouseLogEntity WarehouseLogs { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyRankingDto PreviousMonthRankStat { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyRankingDto CurrentMonthRankStat { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyRankingDto GlobalRankStat { get; set; }

        [Column(TypeName = "jsonb")]
        public FamilyRankingDto CurrentDayRankStat { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}