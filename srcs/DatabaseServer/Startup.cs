using DatabaseServer.Consumers;
using DatabaseServer.Managers;
using DatabaseServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Configuration;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.Database.Extensions;
using ProtoBuf.Grpc.Server;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Game.Configurations;
using WingsEmu.Health.Extensions;

namespace DatabaseServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMqttConfigurationFromEnv();
            new DatabasePlugin().AddDependencies(services);
            services.AddMaintenanceMode();
            services.AddYamlConfigurationHelper();
            services.AddSingleton<CharacterService>();
            services.AddSingleton<CharacterManager>();
            services.AddHostedService(s => s.GetRequiredService<CharacterManager>());
            services.AddSingleton<ICharacterManager>(s => s.GetRequiredService<CharacterManager>());
            services.AddFileConfiguration<GeneralServerConfiguration>();
            services.AddSingleton<AccountWarehouseService>();
            services.AddSingleton<AccountWarehouseManager>();
            services.AddHostedService(s => s.GetRequiredService<AccountWarehouseManager>());
            services.AddSingleton<IAccountWarehouseManager>(s => s.GetRequiredService<AccountWarehouseManager>());

            services.AddSingleton<TimeSpaceService>();
            services.AddSingleton<TimeSpaceManager>();
            services.AddHostedService(s => s.GetRequiredService<TimeSpaceManager>());
            services.AddSingleton<ITimeSpaceManager>(s => s.GetRequiredService<TimeSpaceManager>());

            services.AddSingleton<IRankingManager, RankingManager>();

            services.AddPhoenixLogging();
            services.AddSingleton<AccountService>();

            services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));
            services.TryAddSingleton(typeof(IKeyValueCache<>), typeof(InMemoryKeyValueCache<>));

            services.AddGrpcDbServerServiceClient();
            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

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
                endpoints.MapGrpcService<CharacterService>();
                endpoints.MapGrpcService<AccountWarehouseService>();
                endpoints.MapGrpcService<TimeSpaceService>();
                endpoints.MapGrpcService<AccountService>();
            });
        }
    }
}