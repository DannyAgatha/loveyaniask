// NosEmu
// 


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.Bazaar;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using Plugin.Database.Families;
using Plugin.Database.Mail;
using WingsAPI.Data.BattlePass;
using WingsAPI.Data.Character;
using WingsAPI.Data.Miniland;
using WingsAPI.Data.Prestige;
using WingsAPI.Data.WorldBoss;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Mates;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Respawns;
using WingsEmu.DTOs.Skills;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.Database.Entities.PlayersData
{
    [Table("characters", Schema = DatabaseSchemas.CHARACTERS)]
    public class DbCharacter : BaseAuditableEntity, ILongEntity
    {
        public long AccountId { get; set; }

        public int Act4Dead { get; set; }

        public int Act4Kill { get; set; }
        public int Act4Points { get; set; }

        public int ArenaWinner { get; set; }

        [MaxLength(255)]
        public string Biography { get; set; }

        public bool BuffBlocked { get; set; }
        public ClassType Class { get; set; }
        
        public SubClassType SubClass { get; set; }
        public short Compliment { get; set; }

        public float Dignity { get; set; }

        public bool EmoticonsBlocked { get; set; }

        public bool ExchangeBlocked { get; set; }

        public FactionType Faction { get; set; }

        public bool FamilyRequestBlocked { get; set; }

        public bool FriendRequestBlocked { get; set; }

        public GenderType Gender { get; set; }

        public long Gold { get; set; }

        public bool GroupRequestBlocked { get; set; }

        public HairColorType HairColor { get; set; }

        public HairStyleType HairStyle { get; set; }

        public bool HeroChatBlocked { get; set; }

        public byte HeroLevel { get; set; }

        public long HeroXp { get; set; }

        public int Hp { get; set; }

        public bool HpBlocked { get; set; }

        public bool IsPetAutoRelive { get; set; }

        public bool IsPartnerAutoRelive { get; set; }

        public byte JobLevel { get; set; }

        public long JobLevelXp { get; set; }
        public byte Level { get; set; }

        public long LevelXp { get; set; }

        public int MapId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int MasterPoints { get; set; }

        public int MasterTicket { get; set; }

        public byte MaxPartnerCount { get; set; }

        public byte MaxPetCount { get; set; }

        public bool MinilandInviteBlocked { get; set; }

        [MaxLength(255)]
        public string MinilandMessage { get; set; }

        public short MinilandPoint { get; set; }

        public MinilandState MinilandState { get; set; }

        public bool MouseAimLock { get; set; }

        public int Mp { get; set; }

        [MaxLength(25)]
        public string Prefix { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }

        public bool QuickGetUp { get; set; }

        public bool HideHat { get; set; }

        public bool HideCD { get; set; }

        public bool HideHPMP { get; set; }

        public bool UiBlocked { get; set; }

        public long RagePoint { get; set; }

        public long Reput { get; set; }

        public byte Slot { get; set; }

        public int SpPointsBonus { get; set; }

        public int SpPointsBasic { get; set; }

        public int TalentLose { get; set; }

        public int TalentSurrender { get; set; }

        public int TalentWin { get; set; }

        public bool WhisperBlocked { get; set; }

        public Act5RespawnType Act5RespawnType { get; set; }
        
        public byte TierLevel { get; set; }
        
        public long TierExperience { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterPartnerInventoryItemDto> PartnerInventory { get; set; } = new();

        [Column(TypeName = "jsonb")]
        public List<MateDTO> NosMates { get; set; }

        [Column(TypeName = "jsonb")]
        public List<PartnerWarehouseItemDto> PartnerWarehouse { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterStaticBonusDto> Bonus { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterStaticBuffDto> StaticBuffs { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterQuicklistEntryDto> Quicklist { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterSkillDTO> LearnedSkills { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterTitleDto> Titles { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CompletedScriptsDto> CompletedScripts { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterQuestDto> CompletedQuests { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterQuestDto> ActiveQuests { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterQuestDto> CompletedPeriodicQuests { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterMinilandObjectDto> MinilandObjects { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterReturnDto ReturnPoint { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterInventoryItemDto> Inventory { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CharacterInventoryItemDto> EquippedStuffs { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterLifetimeStatsDto LifetimeStats { get; set; }

        public RespawnType RespawnType { get; set; }

        [Column(TypeName = "jsonb")]
        public List<long> CompletedTimeSpaces { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterRaidRestrictionDto RaidRestrictionDto { get; set; }

        [Column(TypeName = "jsonb")]
        public RainbowBattleLeaverBusterDto RainbowBattleLeaverBusterDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public IcebreakerLeaverBusterDto IcebreakerLeaverBusterDto { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterTrophyFragmentsDto TrophyFragmentsDto { get; set; }

        [Column(TypeName = "jsonb")]
        public List<BattlePassItemDto> BattlePassItemDto { get; set; }

        [Column(TypeName = "jsonb")]
        public List<BattlePassQuestDto> BattlePassQuestDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public BattlePassOptionDto BattlePassOptionDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<CharacterPityDto> PityDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<CharacterFishDto> FishDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<CharacterFishRewardsEarnedDto> FishRewardsEarnedDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<MaxStarTrainerDto> MaxStarTrainerDto { get; set; }

        [Column(TypeName = "jsonb")]
        public List<TrainerQuestDto> TrainerQuestDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public CharacterItemRestrictionDto ItemRestrictionDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<DailyRewardDto> DailyRewardDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<CharacterCookingDto> CharacterCookingDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public CharacterPrestigeDto CharacterPrestigeDto { get; set; }
        
        [Column(TypeName = "jsonb")]
        public List<WorldBossRecordDto> WorldBossRecordsDto { get; set; }
        
        public bool IsAvailableToChangeName { get; set; }
        
        public int FoodValue { get; set; }
        public virtual ICollection<DbBazaarItemEntity> BazaarItem { get; set; }
        public virtual ICollection<DbFamilyMembership> FamilyCharacter { get; set; }
        public virtual ICollection<CharacterRelationEntity> SourceRelations { get; set; }
        public virtual ICollection<CharacterRelationEntity> TargetRelations { get; set; }
        public virtual ICollection<DbCharacterNote> SentNotes { get; set; }
        public virtual ICollection<DbCharacterNote> ReceivedNotes { get; set; }
        public virtual ICollection<DbCharacterMail> ReceivedMails { get; set; }
        public virtual AccountEntity AccountEntity { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}