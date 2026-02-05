using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DatabaseServer.Managers;
using FamilyServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.MQTT;
using Plugin.Database.DB;
using Plugin.Database.Extensions;
using Plugin.Database.Mapping;
using ProtoBuf.Grpc.Client;

namespace DatabaseServer
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
            IDbContextFactory<GameContext> dbContextFactory = host.Services.GetRequiredService<IDbContextFactory<GameContext>>();

            if (!await dbContextFactory.TryMigrateAsync())
            {
                throw new PostgresException("Couldn't migrate the database", "ERROR", "ERROR", "None");
            }

            await host.StartAsync();
            IMessagingService messagingService = host.Services.GetService<IMessagingService>();
            if (messagingService != null)
            {
                await messagingService.StartAsync();
            }

            Log.Info("Database Server started");

            ICharacterManager characterManager = host.Services.GetRequiredService<ICharacterManager>();
            IAccountWarehouseManager accountWarehouseManager = host.Services.GetRequiredService<IAccountWarehouseManager>();
            ITimeSpaceManager timespaceManager = host.Services.GetRequiredService<ITimeSpaceManager>();

            await host.WaitForShutdownAsync(stopService.CancellationToken);
            if (messagingService != null)
            {
                await messagingService.DisposeAsync();
            }

            await characterManager.FlushCharacterSaves();
            await accountWarehouseManager.FlushWarehouseSaves();
            await timespaceManager.FlushTimeSpaceRecords();
        }

        private static void PrintHeader()
        {
            Console.Title = "NosEmu | Database-Server";
            const string text = @"

                  _   _           _____                                         
                 | \ | | ___  ___| ____|_ __ ___  _   _                         
                 |  \| |/ _ \/ __|  _| | '_ ` _ \| | | |                        
                 | |\  | (_) \__ \ |___| | | | | | |_| |                        
                 |_| \_|\___/|___/_____|_| |_| |_|\__,_|                        
  ____        _        _                         ____                           
 |  _ \  __ _| |_ __ _| |__   __ _ ___  ___     / ___|  ___ _ ____   _____ _ __ 
 | | | |/ _` | __/ _` | '_ \ / _` / __|/ _ \____\___ \ / _ \ '__\ \ / / _ \ '__|
 | |_| | (_| | || (_| | |_) | (_| \__ \  __/_____|__) |  __/ |   \ V /  __/ |   
 |____/ \__,_|\__\__,_|_.__/ \__,_|___/\___|    |____/ \___|_|    \_/ \___|_|   
                                                                                                                                    
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
                        s.ListenAnyIP(short.Parse(Environment.GetEnvironmentVariable("DATABASE_SERVER_PORT") ?? "29999"), options => { options.Protocols = HttpProtocols.Http2; });
                    });
                    webBuilder.UseStartup<Startup>();
                });
            return host;
        }
    }
}