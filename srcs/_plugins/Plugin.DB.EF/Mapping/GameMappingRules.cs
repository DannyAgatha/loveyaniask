using Mapster;
using Plugin.Database.Bazaar;
using Plugin.Database.Entities.Account;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Entities.ServerData;
using Plugin.Database.Families;
using Plugin.Database.Mail;
using Plugin.Database.Warehouse;
using WingsAPI.Data.Account;
using WingsAPI.Data.Bazaar;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Data.TimeSpace;
using WingsEmu.DTOs.Mails;
using WingsEmu.DTOs.Relations;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game.Families;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Warehouse;

namespace Plugin.Database.Mapping
{
    public static class GameMappingRules
    {
        public static void InitializeMapping()
        {
            TypeAdapterConfig.GlobalSettings.AllowImplicitDestinationInheritance = true;
            TypeAdapterConfig.GlobalSettings.Default.Settings.Unflattening = false;

            TypeAdapterConfig<AccountEntity, AccountDTO>.NewConfig()
                .ConstructUsing(s => new Account());
            TypeAdapterConfig<AccountDTO, AccountEntity>.NewConfig()
                .Include<Account, AccountEntity>()
                .Ignore(
                    s => s.Character,
                    s => s.AccountPenalties,
                    s => s.AccountBans
                );

            TypeAdapterConfig<AccountWarehouseItemDto, AccountWarehouseItemEntity>.NewConfig()
                .Include<WarehouseItem, AccountWarehouseItemEntity>()
                .Ignore(s => s.Account);

            TypeAdapterConfig<AccountWarehouseItemEntity, AccountWarehouseItemDto>.NewConfig()
                .ConstructUsing(s =>
                    new WarehouseItem());

            // character
            TypeAdapterConfig<DbCharacter, CharacterDTO>.NewConfig();

            TypeAdapterConfig<CharacterDTO, DbCharacter>.NewConfig()
                .Ignore(s => s.AccountEntity,
                    s => s.SentNotes,
                    s => s.ReceivedNotes,
                    s => s.ReceivedMails,
                    s => s.FamilyCharacter,
                    s => s.BazaarItem,
                    s => s.SourceRelations,
                    s => s.TargetRelations);

            TypeAdapterConfig<CharacterRelationEntity, CharacterRelationDTO>
                .NewConfig()
                .Compile();
            // DTO -> Entity
            TypeAdapterConfig<CharacterRelationDTO, CharacterRelationEntity>
                .NewConfig()
                .Compile();


            // Entity -> DTO


            TypeAdapterConfig<DbBazaarItemEntity, BazaarItemDTO>.NewConfig();
            TypeAdapterConfig<BazaarItemDTO, DbBazaarItemEntity>.NewConfig();

            // mails
            TypeAdapterConfig<DbCharacterMail, CharacterMailDto>.NewConfig();
            TypeAdapterConfig<CharacterMailDto, DbCharacterMail>.NewConfig();

            // notes
            TypeAdapterConfig<DbCharacterNote, CharacterNoteDto>.NewConfig();
            TypeAdapterConfig<CharacterNoteDto, DbCharacterNote>.NewConfig();


            // family
            TypeAdapterConfig<DbFamily, FamilyDTO>.NewConfig();
            TypeAdapterConfig<FamilyDTO, DbFamily>.NewConfig()
                .Ignore(
                    s => s.FamilyCharacters,
                    s => s.FamilyLogs
                )
                .Ignore(
                    s => s.FamilyCharacters,
                    s => s.FamilyLogs
                );


            TypeAdapterConfig<DbFamilyMembership, FamilyMembership>.NewConfig();
            TypeAdapterConfig<DbFamilyMembership, FamilyMembershipDto>.NewConfig()
                .ConstructUsing(s => new FamilyMembership());
            TypeAdapterConfig<FamilyMembershipDto, DbFamilyMembership>.NewConfig()
                .Ignore(
                    s => s.DbCharacter,
                    s => s.Family
                )
                .Include<FamilyMembership, DbFamilyMembership>()
                .Ignore(
                    s => s.DbCharacter,
                    s => s.Family
                );

            TypeAdapterConfig<DbFamilyLog, FamilyLogDto>.NewConfig();
            TypeAdapterConfig<FamilyLogDto, DbFamilyLog>.NewConfig();

            // penalty
            TypeAdapterConfig<AccountPenaltyEntity, AccountPenaltyDto>.NewConfig();
            TypeAdapterConfig<AccountPenaltyDto, AccountPenaltyEntity>.NewConfig();

            // bans
            TypeAdapterConfig<AccountBanEntity, AccountBanDto>.NewConfig();
            TypeAdapterConfig<AccountBanDto, AccountBanEntity>.NewConfig();


            TypeAdapterConfig<PartnerSkillDTO, PartnerSkill>.NewConfig();
            TypeAdapterConfig<PartnerSkill, PartnerSkillDTO>.NewConfig();

            TypeAdapterConfig<DbTimeSpaceRecord, TimeSpaceRecordDto>.NewConfig();
            TypeAdapterConfig<TimeSpaceRecordDto, DbTimeSpaceRecord>.NewConfig();

            TypeAdapterConfig.GlobalSettings.Compile();
        }
    }
}