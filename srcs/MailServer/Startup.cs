using MailServer.Consumers;
using MailServer.Managers;
using MailServer.RecurrentJobs;
using MailServer.Services;
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
using ProtoBuf.Grpc.Server;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Health.Extensions;
using WingsEmu.Plugins.DistributedGameEvents.Mails;
using WingsEmu.Plugins.DistributedGameEvents.PlayerEvents;

namespace MailServer
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

            services.TryAddSingleton(typeof(ILongKeyCachedRepository<>), typeof(InMemoryCacheRepository<>));

            services.AddSingleton<MailService>();
            services.AddSingleton<NoteService>();
            services.AddSingleton<MailManager>();
            services.AddSingleton<MailSystem>();
            services.AddHostedService(s => s.GetRequiredService<MailSystem>());

            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

            services.AddMessagePublisher<NoteReceivedMessage>();
            services.AddMessagePublisher<MailReceivedMessage>();
            services.AddMessageSubscriber<PlayerConnectedOnChannelMessage, MailCharacterConnectedMessageConsumer>();
            services.AddMessageSubscriber<PlayerConnectedOnChannelMessage, NoteCharacterConnectedMessageConsumer>();
            services.AddMessagePublisher<MailReceivePendingOnConnectedMessage>();
            services.AddMessagePublisher<NoteReceivePendingOnConnectedMessage>();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<MailService>();
                endpoints.MapGrpcService<NoteService>();
            });
        }
    }
}