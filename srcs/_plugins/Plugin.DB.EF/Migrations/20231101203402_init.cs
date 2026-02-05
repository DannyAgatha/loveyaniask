using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WingsAPI.Data.BattlePass;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Data.Miniland;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Mates;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Respawns;
using WingsEmu.DTOs.Skills;
using WingsEmu.DTOs.Titles;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "accounts");

            migrationBuilder.EnsureSchema(
                name: "_config_auth");

            migrationBuilder.EnsureSchema(
                name: "characters");

            migrationBuilder.EnsureSchema(
                name: "mails");

            migrationBuilder.EnsureSchema(
                name: "families");

            migrationBuilder.EnsureSchema(
                name: "bazaar");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "accounts",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasterAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Authority = table.Column<short>(type: "smallint", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    BankMoney = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimaryAccount = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: true),
                    RegistrationIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "authorized_client_versions",
                schema: "_config_auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientVersion = table.Column<string>(type: "text", nullable: false),
                    ExecutableHash = table.Column<string>(type: "text", nullable: false),
                    DllHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authorized_client_versions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "blacklisted_hardware_ids",
                schema: "_config_auth",
                columns: table => new
                {
                    HardwareId = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    Judge = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklisted_hardware_ids", x => x.HardwareId);
                });

            migrationBuilder.CreateTable(
                name: "families",
                schema: "families",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    Faction = table.Column<byte>(type: "smallint", nullable: false),
                    HeadGender = table.Column<byte>(type: "smallint", nullable: false),
                    Message = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssistantWarehouseAuthorityType = table.Column<byte>(type: "smallint", nullable: false),
                    MemberWarehouseAuthorityType = table.Column<byte>(type: "smallint", nullable: false),
                    AssistantCanGetHistory = table.Column<bool>(type: "boolean", nullable: false),
                    AssistantCanInvite = table.Column<bool>(type: "boolean", nullable: false),
                    AssistantCanNotice = table.Column<bool>(type: "boolean", nullable: false),
                    AssistantCanShout = table.Column<bool>(type: "boolean", nullable: false),
                    MemberCanGetHistory = table.Column<bool>(type: "boolean", nullable: false),
                    Upgrades = table.Column<FamilyUpgradeDto>(type: "jsonb", nullable: true),
                    Achievements = table.Column<FamilyAchievementsDto>(type: "jsonb", nullable: true),
                    Missions = table.Column<FamilyMissionsDto>(type: "jsonb", nullable: true),
                    PreviousMonthRankStat = table.Column<FamilyRankingDto>(type: "jsonb", nullable: true),
                    CurrentMonthRankStat = table.Column<FamilyRankingDto>(type: "jsonb", nullable: true),
                    GlobalRankStat = table.Column<FamilyRankingDto>(type: "jsonb", nullable: true),
                    CurrentDayRankStat = table.Column<FamilyRankingDto>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "time_space_records",
                schema: "characters",
                columns: table => new
                {
                    TimeSpaceId = table.Column<long>(type: "bigint", nullable: false),
                    CharacterName = table.Column<string>(type: "text", nullable: true),
                    Record = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_space_records", x => x.TimeSpaceId);
                });

            migrationBuilder.CreateTable(
                name: "accounts_bans",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    JudgeName = table.Column<string>(type: "text", nullable: true),
                    TargetName = table.Column<string>(type: "text", nullable: true),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    UnlockReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts_bans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_bans_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts_penalties",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    JudgeName = table.Column<string>(type: "text", nullable: true),
                    TargetName = table.Column<string>(type: "text", nullable: true),
                    Start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RemainingTime = table.Column<int>(type: "integer", nullable: true),
                    PenaltyType = table.Column<byte>(type: "smallint", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    UnlockReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts_penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_accounts_penalties_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accounts_warehouse",
                schema: "accounts",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Slot = table.Column<short>(type: "smallint", nullable: false),
                    ItemInstance = table.Column<ItemInstanceDTO>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts_warehouse", x => new { x.AccountId, x.Slot });
                    table.ForeignKey(
                        name: "FK_accounts_warehouse_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                schema: "characters",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Act4Dead = table.Column<int>(type: "integer", nullable: false),
                    Act4Kill = table.Column<int>(type: "integer", nullable: false),
                    Act4Points = table.Column<int>(type: "integer", nullable: false),
                    ArenaWinner = table.Column<int>(type: "integer", nullable: false),
                    Biography = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BuffBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Class = table.Column<byte>(type: "smallint", nullable: false),
                    Compliment = table.Column<short>(type: "smallint", nullable: false),
                    Dignity = table.Column<float>(type: "real", nullable: false),
                    EmoticonsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    ExchangeBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Faction = table.Column<byte>(type: "smallint", nullable: false),
                    FamilyRequestBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    FriendRequestBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Gender = table.Column<byte>(type: "smallint", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false),
                    GroupRequestBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    HairColor = table.Column<byte>(type: "smallint", nullable: false),
                    HairStyle = table.Column<byte>(type: "smallint", nullable: false),
                    HeroChatBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    HeroLevel = table.Column<byte>(type: "smallint", nullable: false),
                    HeroXp = table.Column<long>(type: "bigint", nullable: false),
                    Hp = table.Column<int>(type: "integer", nullable: false),
                    HpBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsPetAutoRelive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPartnerAutoRelive = table.Column<bool>(type: "boolean", nullable: false),
                    JobLevel = table.Column<byte>(type: "smallint", nullable: false),
                    JobLevelXp = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<byte>(type: "smallint", nullable: false),
                    LevelXp = table.Column<long>(type: "bigint", nullable: false),
                    MapId = table.Column<int>(type: "integer", nullable: false),
                    MapX = table.Column<short>(type: "smallint", nullable: false),
                    MapY = table.Column<short>(type: "smallint", nullable: false),
                    MasterPoints = table.Column<int>(type: "integer", nullable: false),
                    MasterTicket = table.Column<int>(type: "integer", nullable: false),
                    MaxPartnerCount = table.Column<byte>(type: "smallint", nullable: false),
                    MaxPetCount = table.Column<byte>(type: "smallint", nullable: false),
                    MinilandInviteBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    MinilandMessage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MinilandPoint = table.Column<short>(type: "smallint", nullable: false),
                    MinilandState = table.Column<byte>(type: "smallint", nullable: false),
                    MouseAimLock = table.Column<bool>(type: "boolean", nullable: false),
                    Mp = table.Column<int>(type: "integer", nullable: false),
                    Prefix = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    Name = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    QuickGetUp = table.Column<bool>(type: "boolean", nullable: false),
                    HideHat = table.Column<bool>(type: "boolean", nullable: false),
                    HideCD = table.Column<bool>(type: "boolean", nullable: false),
                    HideHPMP = table.Column<bool>(type: "boolean", nullable: false),
                    UiBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    RagePoint = table.Column<long>(type: "bigint", nullable: false),
                    Reput = table.Column<long>(type: "bigint", nullable: false),
                    Slot = table.Column<byte>(type: "smallint", nullable: false),
                    SpPointsBonus = table.Column<int>(type: "integer", nullable: false),
                    SpPointsBasic = table.Column<int>(type: "integer", nullable: false),
                    TalentLose = table.Column<int>(type: "integer", nullable: false),
                    TalentSurrender = table.Column<int>(type: "integer", nullable: false),
                    TalentWin = table.Column<int>(type: "integer", nullable: false),
                    WhisperBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Act5RespawnType = table.Column<int>(type: "integer", nullable: false),
                    PartnerInventory = table.Column<List<CharacterPartnerInventoryItemDto>>(type: "jsonb", nullable: true),
                    NosMates = table.Column<List<MateDTO>>(type: "jsonb", nullable: true),
                    PartnerWarehouse = table.Column<List<PartnerWarehouseItemDto>>(type: "jsonb", nullable: true),
                    Bonus = table.Column<List<CharacterStaticBonusDto>>(type: "jsonb", nullable: true),
                    StaticBuffs = table.Column<List<CharacterStaticBuffDto>>(type: "jsonb", nullable: true),
                    Quicklist = table.Column<List<CharacterQuicklistEntryDto>>(type: "jsonb", nullable: true),
                    LearnedSkills = table.Column<List<CharacterSkillDTO>>(type: "jsonb", nullable: true),
                    Titles = table.Column<List<CharacterTitleDto>>(type: "jsonb", nullable: true),
                    CompletedScripts = table.Column<List<CompletedScriptsDto>>(type: "jsonb", nullable: true),
                    CompletedQuests = table.Column<List<CharacterQuestDto>>(type: "jsonb", nullable: true),
                    ActiveQuests = table.Column<List<CharacterQuestDto>>(type: "jsonb", nullable: true),
                    CompletedPeriodicQuests = table.Column<List<CharacterQuestDto>>(type: "jsonb", nullable: true),
                    MinilandObjects = table.Column<List<CharacterMinilandObjectDto>>(type: "jsonb", nullable: true),
                    ReturnPoint = table.Column<CharacterReturnDto>(type: "jsonb", nullable: true),
                    Inventory = table.Column<List<CharacterInventoryItemDto>>(type: "jsonb", nullable: true),
                    EquippedStuffs = table.Column<List<CharacterInventoryItemDto>>(type: "jsonb", nullable: true),
                    LifetimeStats = table.Column<CharacterLifetimeStatsDto>(type: "jsonb", nullable: true),
                    RespawnType = table.Column<int>(type: "integer", nullable: false),
                    CompletedTimeSpaces = table.Column<HashSet<long>>(type: "jsonb", nullable: true),
                    RaidRestrictionDto = table.Column<CharacterRaidRestrictionDto>(type: "jsonb", nullable: true),
                    RainbowBattleLeaverBusterDto = table.Column<RainbowBattleLeaverBusterDto>(type: "jsonb", nullable: true),
                    TrophyFragmentsDto = table.Column<CharacterTrophyFragmentsDto>(type: "jsonb", nullable: true),
                    BattlePassItemDto = table.Column<List<BattlePassItemDto>>(type: "jsonb", nullable: true),
                    BattlePassQuestDto = table.Column<List<BattlePassQuestDto>>(type: "jsonb", nullable: true),
                    BattlePassOptionDto = table.Column<BattlePassOptionDto>(type: "jsonb", nullable: true),
                    PityDto = table.Column<List<CharacterPityDto>>(type: "jsonb", nullable: true),
                    FishDto = table.Column<List<CharacterFishDto>>(type: "jsonb", nullable: true),
                    FishRewardsEarnedDto = table.Column<List<CharacterFishRewardsEarnedDto>>(type: "jsonb", nullable: true),
                    MaxStarTrainerDto = table.Column<List<MaxStarTrainerDto>>(type: "jsonb", nullable: true),
                    TrainerQuestDto = table.Column<List<TrainerQuestDto>>(type: "jsonb", nullable: true),
                    ItemRestrictionDto = table.Column<CharacterItemRestrictionDto>(type: "jsonb", nullable: true),
                    DailyRewardDto = table.Column<List<DailyRewardDto>>(type: "jsonb", nullable: true),
                    IsAvailableToChangeName = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_characters_accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "families_logs",
                schema: "families",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FamilyId = table.Column<long>(type: "bigint", nullable: false),
                    FamilyLogType = table.Column<byte>(type: "smallint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Actor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Argument1 = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Argument2 = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Argument3 = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_families_logs_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "families",
                        principalTable: "families",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "families_warehouses",
                schema: "families",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FamilyId = table.Column<long>(type: "bigint", nullable: false),
                    Slot = table.Column<short>(type: "smallint", nullable: false),
                    ItemInstance = table.Column<ItemInstanceDTO>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families_warehouses", x => new { x.FamilyId, x.Slot });
                    table.ForeignKey(
                        name: "FK_families_warehouses_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "families",
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "families_warehouses_logs",
                schema: "families",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FamilyId = table.Column<long>(type: "bigint", nullable: false),
                    LogEntries = table.Column<List<FamilyWarehouseLogEntryDto>>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families_warehouses_logs", x => x.FamilyId);
                    table.ForeignKey(
                        name: "FK_families_warehouses_logs_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "families",
                        principalTable: "families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters_mails",
                schema: "mails",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SenderName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    MailGiftType = table.Column<int>(type: "integer", nullable: false),
                    ItemInstance = table.Column<ItemInstanceDTO>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters_mails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_characters_mails_characters_ReceiverId",
                        column: x => x.ReceiverId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters_notes",
                schema: "mails",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EquipmentPackets = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsSenderCopy = table.Column<bool>(type: "boolean", nullable: false),
                    IsOpened = table.Column<bool>(type: "boolean", nullable: false),
                    SenderGender = table.Column<byte>(type: "smallint", nullable: false),
                    SenderClass = table.Column<byte>(type: "smallint", nullable: false),
                    SenderHairColor = table.Column<byte>(type: "smallint", nullable: false),
                    SenderHairStyle = table.Column<byte>(type: "smallint", nullable: false),
                    SenderName = table.Column<string>(type: "text", nullable: true),
                    ReceiverName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_characters_notes_characters_ReceiverId",
                        column: x => x.ReceiverId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_characters_notes_characters_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters_relations",
                schema: "characters",
                columns: table => new
                {
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedCharacterId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedName = table.Column<string>(type: "text", nullable: true),
                    RelationType = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters_relations", x => new { x.CharacterId, x.RelatedCharacterId });
                    table.ForeignKey(
                        name: "FK_characters_relations_characters_CharacterId",
                        column: x => x.CharacterId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_characters_relations_characters_RelatedCharacterId",
                        column: x => x.RelatedCharacterId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "families_memberships",
                schema: "families",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<long>(type: "bigint", nullable: true),
                    FamilyId = table.Column<long>(type: "bigint", nullable: false),
                    Authority = table.Column<byte>(type: "smallint", nullable: false),
                    DailyMessage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<byte>(type: "smallint", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastOnlineDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_families_memberships_characters_CharacterId",
                        column: x => x.CharacterId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_families_memberships_families_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "families",
                        principalTable: "families",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "items",
                schema: "bazaar",
                columns: table => new
                {
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    SoldAmount = table.Column<int>(type: "integer", nullable: false),
                    PricePerItem = table.Column<long>(type: "bigint", nullable: false),
                    SaleFee = table.Column<long>(type: "bigint", nullable: false),
                    IsPackage = table.Column<bool>(type: "boolean", nullable: false),
                    UsedMedal = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DayExpiryAmount = table.Column<short>(type: "smallint", nullable: false),
                    ItemInstance = table.Column<ItemInstanceDTO>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_items_characters_CharacterId",
                        column: x => x.CharacterId,
                        principalSchema: "characters",
                        principalTable: "characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_bans_AccountId",
                schema: "accounts",
                table: "accounts_bans",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_penalties_AccountId",
                schema: "accounts",
                table: "accounts_penalties",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_characters_AccountId",
                schema: "characters",
                table: "characters",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_characters_mails_ReceiverId",
                schema: "mails",
                table: "characters_mails",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_characters_notes_ReceiverId",
                schema: "mails",
                table: "characters_notes",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_characters_notes_SenderId",
                schema: "mails",
                table: "characters_notes",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_characters_relations_RelatedCharacterId",
                schema: "characters",
                table: "characters_relations",
                column: "RelatedCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_families_logs_FamilyId",
                schema: "families",
                table: "families_logs",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_families_memberships_CharacterId",
                schema: "families",
                table: "families_memberships",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_families_memberships_FamilyId",
                schema: "families",
                table: "families_memberships",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_items_CharacterId",
                schema: "bazaar",
                table: "items",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts_bans",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "accounts_penalties",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "accounts_warehouse",
                schema: "accounts");

            migrationBuilder.DropTable(
                name: "authorized_client_versions",
                schema: "_config_auth");

            migrationBuilder.DropTable(
                name: "blacklisted_hardware_ids",
                schema: "_config_auth");

            migrationBuilder.DropTable(
                name: "characters_mails",
                schema: "mails");

            migrationBuilder.DropTable(
                name: "characters_notes",
                schema: "mails");

            migrationBuilder.DropTable(
                name: "characters_relations",
                schema: "characters");

            migrationBuilder.DropTable(
                name: "families_logs",
                schema: "families");

            migrationBuilder.DropTable(
                name: "families_memberships",
                schema: "families");

            migrationBuilder.DropTable(
                name: "families_warehouses",
                schema: "families");

            migrationBuilder.DropTable(
                name: "families_warehouses_logs",
                schema: "families");

            migrationBuilder.DropTable(
                name: "items",
                schema: "bazaar");

            migrationBuilder.DropTable(
                name: "time_space_records",
                schema: "characters");

            migrationBuilder.DropTable(
                name: "families",
                schema: "families");

            migrationBuilder.DropTable(
                name: "characters",
                schema: "characters");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "accounts");
        }
    }
}
