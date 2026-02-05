// NosEmu
// 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using PhoenixLib.DAL;
using Plugin.Database.Bazaar;
using Plugin.Database.DAOs;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Extensions;
using Plugin.Database.Families;
using Plugin.Database.Mail;
using Plugin.Database.Mapping;
using Plugin.Database.Warehouse;
using System;
using WingsAPI.Data.Account;
using WingsAPI.Data.Bazaar;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Data.TimeSpace;
using WingsAPI.Data.Warehouse;
using WingsAPI.Plugins;
using WingsEmu.DTOs.Bazaar;
using WingsEmu.DTOs.Mails;
using WingsEmu.DTOs.Relations;

namespace Plugin.Database
{
    public class DatabasePlugin : IDependencyInjectorPlugin
    {
        public string Name => nameof(DatabasePlugin);

        public void AddDependencies(IServiceCollection services)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            // Obselete but still suported see #https://www.npgsql.org/doc/types/jsonnet.html?tabs=global
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet(settings: new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            services.AddDbContextFactory<GameContext>((serviceProvider, options) =>
            {
                DatabaseConfiguration conf = serviceProvider.GetRequiredService<DatabaseConfiguration>();
                options
                    .UseNpgsql(conf.ToString(), providerOptions => { providerOptions.EnableRetryOnFailure(); })
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .EnableServiceProviderCaching()
                    .ConfigureWarnings(s => s.Log(
                        (RelationalEventId.CommandExecuting, LogLevel.Debug),
                        (RelationalEventId.CommandExecuted, LogLevel.Debug)
                    ));
            });
            services.AddSingleton<DatabaseConfiguration>();
            services.AddTransient(typeof(IMapper<,>), typeof(MapsterMapper<,>));

            // accounts
            services.AddAsyncLongRepository<AccountDTO, AccountEntity>();
            services.AddTransient<IAccountDAO, AccountDAO>();

            // accounts warehouse
            services.AddTransient<IAccountWarehouseItemDao, AccountWarehouseItemItemDao>();

            // accounts bans
            services.AddAsyncLongRepository<AccountBanDto, AccountBanEntity>();
            services.TryAddTransient<IAccountBanDao, AccountBanDao>();

            // accounts penalties
            services.AddAsyncLongRepository<AccountPenaltyDto, AccountPenaltyEntity>();
            services.TryAddTransient<IAccountPenaltyDao, AccountPenaltyDao>();

            // bazaar
            services.AddAsyncLongRepository<BazaarItemDTO, DbBazaarItemEntity>();
            services.TryAddTransient<IBazaarItemDAO, BazaarItemDAO>();


            // character
            services.AddAsyncLongRepository<CharacterDTO, DbCharacter>();
            services.TryAddTransient<ICharacterDAO, CharacterDAO>();

            // timespaces records
            services.AddSingleton<ITimeSpaceRecordDao, TimeSpaceRecordDao>();

            // relations
            services.TryAddTransient<ICharacterRelationDAO, CharacterRelationDAO>();

            // mails
            services.AddAsyncLongRepository<CharacterMailDto, DbCharacterMail>();
            services.TryAddTransient<ICharacterMailDao, CharacterMailDao>();

            // notes
            services.AddAsyncLongRepository<CharacterNoteDto, DbCharacterNote>();
            services.TryAddTransient<ICharacterNoteDao, CharacterNoteDao>();


            // families
            services.AddAsyncLongRepository<FamilyDTO, DbFamily>();
            services.TryAddTransient<IFamilyDAO, FamilyDAO>();

            // families membership
            services.AddAsyncLongRepository<FamilyMembershipDto, DbFamilyMembership>();
            services.TryAddTransient<IFamilyMembershipDao, FamilyMembershipDao>();

            // families logs
            services.AddAsyncLongRepository<FamilyLogDto, DbFamilyLog>();
            services.TryAddTransient<IFamilyLogDAO, FamilyLogDAO>();

            // family warehouses
            services.TryAddTransient<IFamilyWarehouseItemDao, FamilyWarehouseItemDao>();
            services.TryAddTransient<IFamilyWarehouseLogDao, FamilyWarehouseLogDao>();
        }
    }
}