using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using ProtoBuf.Grpc.Client;
using WingsAPI.Data.GameData;

namespace TranslationServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            PrintHeader();
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var stopService = new DockerGracefulStopService();
            using IHost host = CreateHostBuilder(args).Build();
            {
                IServiceProvider services = host.Services;
                IResourceLoader<GenericTranslationDto> loader = services.GetRequiredService<IResourceLoader<GenericTranslationDto>>();
                await loader.LoadAsync();

                Log.Warn("Starting host...");
                await host.StartAsync();
                //
                // IMessagingService messagingService =  host.Services.GetRequiredService<IMessagingService>();
                // await messagingService.StartAsync();

                await host.WaitForShutdownAsync(stopService.CancellationToken);
                // await messagingService.DisposeAsync();
            }
        }

        private static void PrintHeader()
        {
            Console.Title = "NosEmu | Translations-Server";
            const string text = @"

                         _   _           _____                                               
                        | \ | | ___  ___| ____|_ __ ___  _   _                               
                        |  \| |/ _ \/ __|  _| | '_ ` _ \| | | |                              
                        | |\  | (_) \__ \ |___| | | | | | |_| |                              
                        |_| \_|\___/|___/_____|_| |_| |_|\__,_|                              
  _____                    _       _   _                      ____                           
 |_   _| __ __ _ _ __  ___| | __ _| |_(_) ___  _ __  ___     / ___|  ___ _ ____   _____ _ __ 
   | || '__/ _` | '_ \/ __| |/ _` | __| |/ _ \| '_ \/ __|____\___ \ / _ \ '__\ \ / / _ \ '__|
   | || | | (_| | | | \__ \ | (_| | |_| | (_) | | | \__ \_____|__) |  __/ |   \ V /  __/ |   
   |_||_|  \__,_|_| |_|___/_|\__,_|\__|_|\___/|_| |_|___/    |____/ \___|_|    \_/ \___|_|   
                                                                                                                                                                                                                           
";
            string separator = new('=', Console.WindowWidth);
            string logo = text.Split('\n').Select(s => string.Format("{0," + (Console.WindowWidth / 2 + s.Length / 2) + "}\n", s))
                .Aggregate("", (current, i) => current + i);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(separator + logo + $"Version: {Assembly.GetExecutingAssembly().GetName().Version}\n" + separator);
            Console.ForegroundColor = ConsoleColor.White;
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(s =>
                    {
                        s.ListenAnyIP(short.Parse(Environment.GetEnvironmentVariable("TRANSLATION_SERVER_PORT") ?? "19999"), options => { options.Protocols = HttpProtocols.Http2; });
                    });
                    webBuilder.UseStartup<Startup>();
                });
            return host;
        }
    }
}