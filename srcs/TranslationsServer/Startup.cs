using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Configuration;
using PhoenixLib.ServiceBus.Extensions;
using ProtoBuf.Grpc.Server;
using TranslationServer.Loader;
using TranslationServer.Services;
using WingsAPI.Communication.Translations;
using WingsAPI.Data.GameData;
using WingsEmu.Health.Extensions;

namespace TranslationServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMqttConfigurationFromEnv();
            services.AddMaintenanceMode();

            services.AddCodeFirstGrpc(config =>
            {
                config.MaxReceiveMessageSize = null;
                config.MaxSendMessageSize = null;
                config.EnableDetailedErrors = true;
            });

            services.AddSingleton(s => new TranslationsFileLoaderOptions
            {
                TranslationsPath = Environment.GetEnvironmentVariable("TRANSLATIONS_PATH") ?? "translations"
            });
            services.AddSingleton<GrpcGameLanguageService>();
            services.AddSingleton<IResourceLoader<GenericTranslationDto>, GenericTranslationFileLoader>();
            services.AddSingleton<ITranslationService, GrpcGameLanguageService>();

            services.AddYamlConfigurationHelper();
            services.AddFileConfiguration<BannedNamesConfiguration>("banned_names_configuration");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GrpcGameLanguageService>(); });
        }
    }
}