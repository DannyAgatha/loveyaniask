using System;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.DAL.Redis;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using WingsAPI.Communication.Caligor;
using WingsAPI.Communication.Compliments;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Icebreaker;
using WingsAPI.Communication.InstantBattle;
using WingsAPI.Communication.LandOfLife;
using WingsAPI.Communication.Miniland;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.Quests;
using WingsAPI.Communication.Raid;
using WingsAPI.Communication.RainbowBattle;
using WingsAPI.Communication.UnderWaterShowDown;
using WingsAPI.Communication.WorldBoss;
using WingsAPI.Plugins;
using WingsAPI.Plugins.Exceptions;
using WingsEmu.ClusterScheduler.Service;
using WingsEmu.ClusterScheduler.Utility;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Game;
using WingsEmu.Health.Extensions;
using WingsEmu.Game.Alzanor.Communication;
using WingsEmu.Plugins.DistributedGameEvents;
using WingsEmu.Plugins.DistributedGameEvents.BotMessages;

namespace WingsEmu.ClusterScheduler
{
    public class Startup
    {
        private static ServiceProvider GetPluginsProvider()
        {
            var pluginBuilder = new ServiceCollection();
            pluginBuilder.AddTransient<IDependencyInjectorPlugin, ScheduledEventPublisherCorePlugin>();
            return pluginBuilder.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            new DatabasePlugin().AddDependencies(services);
            using ServiceProvider plugins = GetPluginsProvider();
            foreach (IDependencyInjectorPlugin plugin in plugins.GetServices<IDependencyInjectorPlugin>())
            {
                try
                {
                    Log.Debug($"[PLUGIN_LOADER] Loading generic plugin {plugin.Name}...");
                    plugin.AddDependencies(services);
                }
                catch (PluginException e)
                {
                    Log.Error($"{plugin.Name} : plugin.OnLoad", e);
                }
            }
            
            services.AddPhoenixLogging();
            services.AddEventPipeline();
            services.AddEventHandlersInAssembly<Startup>();
            services.AddSingleton<IRandomGenerator, RandomGenerator>();
            services.AddHangfire(configuration =>
            {
                configuration.UseColouredConsoleLogProvider();
                configuration.UseMemoryStorage();
            });
            services.AddMaintenanceMode();
            //services.AddFileConfiguration<InstantBattleStartFileConfiguration>();
            services.AddMqttConfigurationFromEnv();
            services.AddMessagePublisher<BotMessageMessage>();
            services.AddMessagePublisher<InstantBattleStartMessage>();
            services.AddMessagePublisher<RainbowBattleStartMessage>();
            services.AddMessagePublisher<RainbowBattleLeaverBusterResetMessage>();
            services.AddMessagePublisher<RankingRefreshMessage>();
            services.AddMessagePublisher<CaligorStartMessage>();
            services.AddMessagePublisher<Act4PercentageMessage>();
            services.AddMessagePublisher<FamilyMissionsResetMessage>();
            services.AddMessagePublisher<QuestDailyRefreshMessage>();
            services.AddMessagePublisher<MinigameRefreshProductionPointsMessage>();
            services.AddMessagePublisher<RaidRestrictionRefreshMessage>();
            services.AddMessagePublisher<LandOfLifeRestrictionRefreshMessage>();
            services.AddMessagePublisher<SpecialistPointsRefreshMessage>();
            services.AddMessagePublisher<ComplimentsMonthlyRefreshMessage>();
            services.AddMessagePublisher<UnderWaterShowdownStartMessage>();
            services.AddMessagePublisher<IcebreakerStartMessage>();
            services.AddMessagePublisher<AlzanorStartMessage>();

            services.TryAddConnectionMultiplexerFromEnv();

            services.AddHangfireServer();
            services.AddGrpcDbServerServiceClient();

            /*
             * WARM UP SERVICES
             */
            services.AddHostedService<InstantBattleCronScheduler>();
            services.AddHostedService<RainbowBattleCronScheduler>();
            services.AddHostedService<RainbowBattleLeaverBusterCronScheduler>();
            services.AddHostedService<MinigameProductionRefreshCronScheduler>();
            services.AddHostedService<QuestDailyRefreshCronScheduler>();
            services.AddHostedService<ComplimentsMonthlyRefreshCronScheduler>();
            services.AddHostedService<SpecialistPointsRefreshCronScheduler>();
            services.AddHostedService<FamilyMissionsResetCronScheduler>();
            services.AddHostedService<RankingRefreshCronScheduler>();
            services.AddHostedService<RaidRestrictionRefreshCronScheduler>();
            services.AddHostedService<ItemRestrictionRefreshCronScheduler>();
            services.AddHostedService<RewardRestrictionRefreshCronScheduler>();
            services.AddHostedService<LandOfLifeRestrictionRefreshCronScheduler>();
            services.AddHostedService<Act4CaligorCronScheduler>();
            services.AddHostedService<Act4PercentageMessageCron>();
            services.AddHostedService<UnderWaterShowdownCronScheduler>();
            services.AddHostedService<IcebreakerCronScheduler>();
            services.AddHostedService<AlzanorCronScheduler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            GlobalConfiguration.Configuration.UseActivator(new HangfireJobActivator(serviceProvider));

            app.UseRouting();

            app.UseHangfireServer();
            app.UseHangfireDashboard();
        }
    }
}