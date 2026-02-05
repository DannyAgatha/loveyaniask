using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DiscordNotifier.Managers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus.MQTT;

namespace DiscordNotifier
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            PrintHeader();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var stopService = new DockerGracefulStopService();
            using IHost host = CreateHostBuilder(args).Build();
            {
                IServiceProvider services = host.Services;
                await host.StartAsync();
                IMessagingService messagingService = services.GetRequiredService<IMessagingService>();
                await messagingService.StartAsync();

                ItemManager itemManager = services.GetRequiredService<ItemManager>();
                await itemManager.CacheClientItems();

                await host.WaitForShutdownAsync(stopService.CancellationToken);
                await messagingService.DisposeAsync();
            }
        }

        private static void PrintHeader()
        {
            Console.Title = "NosEmu | Discord-Notifier";
            const string text = @"
                 _   _           _____                                    
                | \ | | ___  ___| ____|_ __ ___  _   _                    
                |  \| |/ _ \/ __|  _| | '_ ` _ \| | | |                   
                | |\  | (_) \__ \ |___| | | | | | |_| |                   
                |_| \_|\___/|___/_____|_| |_| |_|\__,_|                   
  ____  _                       _       _   _       _   _  __ _           
 |  _ \(_)___  ___ ___  _ __ __| |     | \ | | ___ | |_(_)/ _(_) ___ _ __ 
 | | | | / __|/ __/ _ \| '__/ _` |_____|  \| |/ _ \| __| | |_| |/ _ \ '__|
 | |_| | \__ \ (_| (_) | | | (_| |_____| |\  | (_) | |_| |  _| |  __/ |   
 |____/|_|___/\___\___/|_|  \__,_|     |_| \_|\___/ \__|_|_| |_|\___|_|   
                                                                          
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
                    webBuilder.ConfigureKestrel(s => { s.ListenAnyIP(28000); });
                    webBuilder.UseStartup<Startup>();
                });
            return host;
        }
    }
}