using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FamilyServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.MQTT;
using Plugin.Database.Mapping;
using ProtoBuf.Grpc.Client;

namespace MailServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            PrintHeader();
            NonGameMappingRules.InitializeMapping();
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            using var stopService = new DockerGracefulStopService();
            using IHost host = CreateHostBuilder(args).Build();
            {
                await host.StartAsync();
                IMessagingService messagingService = host.Services.GetRequiredService<IMessagingService>();
                await messagingService.StartAsync();
                IServiceProvider services = host.Services;
                Log.Info("MailServer is working...");
                await host.WaitForShutdownAsync(stopService.CancellationToken);
                await messagingService.DisposeAsync();
            }
        }


        private static void PrintHeader()
        {
            Console.Title = "NosEmu | Mail-Server";
            const string text = @"

        _   _           _____                          
       | \ | | ___  ___| ____|_ __ ___  _   _          
       |  \| |/ _ \/ __|  _| | '_ ` _ \| | | |         
       | |\  | (_) \__ \ |___| | | | | | |_| |         
       |_| \_|\___/|___/_____|_| |_| |_|\__,_|         
  __  __       _ _      ____                           
 |  \/  | __ _(_) |    / ___|  ___ _ ____   _____ _ __ 
 | |\/| |/ _` | | |____\___ \ / _ \ '__\ \ / / _ \ '__|
 | |  | | (_| | | |_____|__) |  __/ |   \ V /  __/ |   
 |_|  |_|\__,_|_|_|    |____/ \___|_|    \_/ \___|_|   
                                                                                                                                   
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
                        s.ListenAnyIP(short.Parse(Environment.GetEnvironmentVariable("MAIL_SERVER_PORT") ?? "27777"), options => { options.Protocols = HttpProtocols.Http2; });
                    });
                    webBuilder.UseStartup<Startup>();
                });
            return host;
        }
    }
}