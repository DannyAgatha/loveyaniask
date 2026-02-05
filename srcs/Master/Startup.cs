using System;
using Master.Consumers;
using Master.Managers;
using Master.Proxies;
using Master.RecurrentJobs;
using Master.Services.Maintenance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.DAL;
using PhoenixLib.DAL.Redis;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler.ReactiveX;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.Database.DB;
using Plugin.Database.Mapping;
using ProtoBuf.Grpc.Server;
using RandN;
using RandN.Compat;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Game;
using WingsEmu.Health;
using WingsEmu.Master;
using WingsEmu.Master.Sessions;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace Master
{
    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Random Local = RandomShim.Create(SmallRng.Create());

        public int RandomNumber(int min, int max)
        {
            if (min > max)
            {
                return RandomNumber(max, min);
            }

            return min == max ? max : Local.Next(min, max);
        }

        public int RandomNumber(int max) => RandomNumber(0, max);

        public int RandomNumber() => RandomNumber(0, 100);
        
        public double RandomNumber(double min, double max)
        {
            if (min > max)
            {
                return RandomNumber(max, min);
            }

            return Local.NextDouble() * (max - min) + min;
        }
    }

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScheduler();
            services.AddCron();
            services.AddPhoenixLogging();

            if (bool.TryParse(Environment.GetEnvironmentVariable("WORLD_PULSE_SYSTEM") ?? "false", out bool pulseSystem) && pulseSystem)
            {
                services.AddHostedService<GameChannelHeartbeatService>();
            }

            services.AddSingleton<IRandomGenerator, RandomGenerator>();
            services.AddSingleton(s => RedisConfiguration.FromEnv());
            services.AddSingleton(s => s.GetRequiredService<RedisConfiguration>().GetConnectionMultiplexer());
            services.AddSingleton<DatabaseConfiguration>();
            services.AddSingleton<EncryptionKeyFactory>();
            services.AddSingleton<ISessionManager, RedisSessionManager>();
            services.AddSingleton<SessionService>();
            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });
            services.AddSingleton<WorldServerManager>();
            services.AddSingleton<ClusterCharacterManager>();
            services.AddSingleton<ClusterCharacterService>();

            services.AddMqttConfigurationFromEnv();
            services.AddMessagePublisher<WorldServerShutdownMessage>();
            services.AddMessagePublisher<KickAccountMessage>();

            services.AddSingleton<IStatusManager, StatusManager>();
            services.AddHostedService(s => s.GetRequiredService<IStatusManager>() as StatusManager);
            services.AddSingleton<GrpcClusterStatusService>();
            services.AddMessagePublisher<ServiceDownMessage>();
            services.AddMessageSubscriber<ServiceStatusUpdateMessage, StatusRefreshMessageConsumer>();
            services.AddMessagePublisher<ServiceMaintenanceActivateMessage>();
            services.AddMessagePublisher<ServiceMaintenanceDeactivateMessage>();
            services.AddMessagePublisher<ServiceMaintenanceNotificationMessage>();
            services.AddMessagePublisher<ServiceKickAllMessage>();
            services.AddMessagePublisher<ServiceFlushAllMessage>();
            services.AddMessageSubscriber<PlayerConnectedOnChannelMessage, PlayerConnectedOnChannelMessageConsumer>();
            services.AddMessageSubscriber<PlayerDisconnectedChannelMessage, PlayerDisconnectedChannelMessageConsumer>();

            new DatabasePlugin().AddDependencies(services);
            services.AddTransient(typeof(IMapper<,>), typeof(MapsterMapper<,>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ServerApiService>();
                endpoints.MapGrpcService<SessionService>();
                endpoints.MapGrpcService<GrpcClusterStatusService>();
                endpoints.MapGrpcService<ClusterCharacterService>();
            });
        }
    }
}