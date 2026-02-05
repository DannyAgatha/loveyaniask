using BazaarServer.Consumers;
using BazaarServer.Managers;
using BazaarServer.RecurrentJobs;
using BazaarServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using Plugin.ResourceLoader;
using ProtoBuf.Grpc.Server;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Health.Extensions;

namespace BazaarServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMqttConfigurationFromEnv();
            services.AddMaintenanceMode();
            services.AddEventPipeline();
            services.AddEventHandlersInAssembly<Startup>();
            new DatabasePlugin().AddDependencies(services);
            new FileResourceLoaderPlugin().AddDependencies(services);

            services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));
            services.TryAddSingleton(typeof(IUuidKeyCachedRepository<>), typeof(InMemoryUuidCacheRepository<>));

            services.AddGrpcBazaarServiceClient();
            services.AddSingleton<BazaarManager>();
            services.AddSingleton<BazaarSearchManager>();
            services.AddSingleton<BazaarService>();
            services.AddSingleton<BazaarSystem>();
            services.AddHostedService(s => s.GetRequiredService<BazaarSystem>());
            services.AddPhoenixLogging();

            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

            services.AddMessageSubscriber<ServiceMaintenanceNotificationMessage, ServiceMaintenanceNotificationMessageConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapGrpcService<BazaarService>(); });
        }
    }
}