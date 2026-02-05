using FamilyServer.Achievements;
using FamilyServer.Consumers;
using FamilyServer.Logs;
using FamilyServer.Managers;
using FamilyServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Configuration;
using PhoenixLib.DAL.Redis;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.Database.Extensions;
using Plugin.Database.Families;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Messages;
using ProtoBuf.Grpc.Server;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Services.Messages;
using WingsAPI.Data.Families;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Health.Extensions;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;
using FamilyAchievementManager = FamilyServer.Achievements.FamilyAchievementManager;

namespace FamilyServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMqttConfigurationFromEnv();
            services.AddEventPipeline();
            services.AddEventHandlersInAssembly<Startup>();
            services.AddMaintenanceMode();
            services.TryAddConnectionMultiplexerFromEnv();
            services.TryAddSingleton<IExpirableLockService, RedisCheckableLock>();
            services.AddPhoenixLogging();


            new DatabasePlugin().AddDependencies(services);

            services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));

            services.AddSingleton<FamilyManager>();
            services.AddSingleton<FamilyMembershipManager>();
            services.AddSingleton<IFamilyWarehouseManager, FamilyWarehouseManager>();
            services.AddSingleton<FamilyWarehouseLogManager>();
            services.AddFileConfiguration<GeneralServerConfiguration>();

            services.AddSingleton<FamilyService>();
            services.AddSingleton<IFamilyService, FamilyService>();
            services.AddSingleton<FamilyInvitationService>();
            services.AddSingleton<FamilyWarehouseService>();

            services.AddSingleton<FamilySystem>();
            services.AddHostedService(s => s.GetRequiredService<FamilySystem>());

            services.AddSingleton<IFamilyLogDAO, FamilyLogDAO>();
            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

            services.AddYamlConfigurationHelper();
            services.AddFileConfiguration<FamilyConfiguration>();
            
            services.TryAddSingleton<FamilyExperienceManager>();
            services.AddHostedService(s => s.GetRequiredService<FamilyExperienceManager>());

            services.TryAddSingleton<FamilyLogManager>();
            services.AddHostedService(s => s.GetRequiredService<FamilyLogManager>());

            services.AddMessageSubscriber<PlayerConnectedOnChannelMessage, FamilyCharacterConnectMessageConsumer>();
            services.AddMessageSubscriber<PlayerDisconnectedChannelMessage, FamilyCharacterDisconnectMessageConsumer>();
            services.AddMessageSubscriber<FamilyDeclareLogsMessage, FamilyDeclareLogsMessageConsumer>();
            services.AddMessageSubscriber<FamilyDeclareExperienceGainedMessage, FamilyDeclareExperienceGainedMessageConsumer>();
            services.AddMessageSubscriber<FamilyNoticeMessage, FamilyNoticeMessageConsumer>();
            services.AddMessageSubscriber<FamilyMemberTodayMessage, FamilyMemberTodayMessageConsumer>();
            services.AddMessageSubscriber<FamilyHeadSexMessage, FamilyHeadSexMessageConsumer>();
            services.AddMessageSubscriber<FamilyMissionsResetMessage, FamilyMissionsResetMessageConsumer>();

            services.AddMessagePublisher<FamilyMemberAddedMessage>();
            services.AddMessagePublisher<FamilyCreatedMessage>();
            services.AddMessagePublisher<FamilyDisbandMessage>();
            services.AddMessagePublisher<FamilyChangeFactionMessage>();
            services.AddMessagePublisher<FamilyMemberUpdateMessage>();
            services.AddMessagePublisher<FamilyMemberRemovedMessage>();
            services.AddMessagePublisher<FamilyAcknowledgeLogsMessage>();
            services.AddMessagePublisher<FamilyAcknowledgeExperienceGainedMessage>();
            services.AddMessagePublisher<FamilyUpdateMessage>();
            services.AddMessagePublisher<FamilyCharacterJoinMessage>();
            services.AddMessagePublisher<FamilyCharacterLeaveMessage>();
            services.AddMessagePublisher<FamilyWarehouseItemUpdateMessage>();
            services.AddMessagePublisher<FamilyWarehouseLogAddMessage>();
            services.AddMessagePublisher<FamilyCreateBuffMessage>();

            // achievements
            services.AddFileConfiguration<FamilyAchievementsConfiguration>();
            services.AddFileConfiguration<FamilyMissionsConfiguration>("family_missions_configuration");
            services.AddMessageSubscriber<FamilyAchievementIncrementMessage, FamilyAchievementIncrementMessageConsumer>();
            services.AddMessageSubscriber<FamilyMissionIncrementMessage, FamilyMissionIncrementMessageConsumer>();
            services.AddMessagePublisher<FamilyAchievementUnlockedMessage>();
            services.TryAddSingleton<FamilyAchievementManager>();
            services.AddHostedService(s => s.GetRequiredService<FamilyAchievementManager>());

            services.AddMessageSubscriber<ServiceFlushAllMessage, ServiceFlushAllMessageConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseZEfCoreExtensions();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<FamilyService>();
                endpoints.MapGrpcService<FamilyInvitationService>();
                endpoints.MapGrpcService<FamilyWarehouseService>();
            });
        }
    }
}