using Mapster;
using Plugin.Database.Bazaar;
using Plugin.Database.Entities.Account;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Entities.ServerData;
using Plugin.Database.Families;
using Plugin.Database.Mail;
using WingsAPI.Data.Account;
using WingsAPI.Data.Bazaar;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Data.TimeSpace;
using WingsEmu.DTOs.Mails;
using WingsEmu.DTOs.Relations;

namespace Plugin.Database.Mapping
{
    public static class NonGameMappingRules
    {
        public static void InitializeMapping()
        {
            TypeAdapterConfig.GlobalSettings.AllowImplicitDestinationInheritance = true;


            TypeAdapterConfig<AccountEntity, AccountDTO>.NewConfig();
            TypeAdapterConfig<AccountDTO, AccountEntity>.NewConfig();

            // character
            TypeAdapterConfig<DbCharacter, CharacterDTO>.NewConfig();
            TypeAdapterConfig<CharacterDTO, DbCharacter>.NewConfig();

            TypeAdapterConfig<CharacterRelationEntity, CharacterRelationDTO>.NewConfig();
            // DTO -> Entity
            TypeAdapterConfig<CharacterRelationDTO, CharacterRelationEntity>.NewConfig();


            // Entity -> DTO


            TypeAdapterConfig<DbBazaarItemEntity, BazaarItemDTO>.NewConfig();
            TypeAdapterConfig<BazaarItemDTO, DbBazaarItemEntity>.NewConfig();

            TypeAdapterConfig<DbCharacterMail, CharacterMailDto>.NewConfig();
            TypeAdapterConfig<CharacterMailDto, DbCharacterMail>.NewConfig();


            // family
            TypeAdapterConfig<DbFamily, FamilyDTO>.NewConfig();
            TypeAdapterConfig<FamilyDTO, DbFamily>.NewConfig();

            TypeAdapterConfig<DbFamilyMembership, FamilyMembershipDto>.NewConfig();
            TypeAdapterConfig<FamilyMembershipDto, DbFamilyMembership>.NewConfig();

            TypeAdapterConfig<DbFamilyLog, FamilyLogDto>.NewConfig();
            TypeAdapterConfig<FamilyLogDto, DbFamilyLog>.NewConfig();

            // penalty
            TypeAdapterConfig<AccountPenaltyEntity, AccountPenaltyDto>.NewConfig();
            TypeAdapterConfig<AccountPenaltyDto, AccountPenaltyEntity>.NewConfig();

            // bans
            TypeAdapterConfig<AccountBanEntity, AccountBanDto>.NewConfig();
            TypeAdapterConfig<AccountBanDto, AccountBanEntity>.NewConfig();

            // time-space records
            TypeAdapterConfig<DbTimeSpaceRecord, TimeSpaceRecordDto>.NewConfig();
            TypeAdapterConfig<TimeSpaceRecordDto, DbTimeSpaceRecord>.NewConfig();

            // logs
            TypeAdapterConfig.GlobalSettings.Compile();
        }
    }
}