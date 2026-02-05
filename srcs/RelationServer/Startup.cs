using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Database;
using ProtoBuf.Grpc.Server;
using RelationServer.Consumer;
using RelationServer.Services;
using WingsEmu.Health.Extensions;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;
using WingsEmu.Plugins.DistributedGameEvents.Relation;

namespace RelationServer
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
            services.AddPhoenixLogging();
            new DatabasePlugin().AddDependencies(services);

            services.AddSingleton<RelationService>();

            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

            services.AddMessagePublisher<RelationCharacterJoinMessage>();
            services.AddMessagePublisher<RelationCharacterLeaveMessage>();
            services.AddMessagePublisher<RelationCharacterAddMessage>();
            services.AddMessagePublisher<RelationCharacterRemoveMessage>();
            services.AddMessageSubscriber<PlayerConnectedOnChannelMessage, RelationCharacterConnectMessageConsumer>();
            services.AddMessageSubscriber<PlayerDisconnectedChannelMessage, RelationCharacterDisconnectMessageConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapGrpcService<RelationService>(); });
        }
    }
}