using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using LoginServer.Handlers;
using LoginServer.Network;
using LoginServer.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.MQTT;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Packets;
using WingsEmu.Packets.ClientPackets;

namespace LoginServer
{
    public class Program
    {
        private static void PrintHeader()
        {
            Console.Title = "NosEmu | Login";
            const string text = @"

              _   _           _____                            
             | \ | | ___  ___| ____|_ __ ___  _   _            
             |  \| |/ _ \/ __|  _| | '_ ` _ \| | | |           
             | |\  | (_) \__ \ |___| | | | | | |_| |           
             |_| \_|\___/|___/_____|_| |_| |_|\__,_|           
  _                _            ____                           
 | |    ___   __ _(_)_ __      / ___|  ___ _ ____   _____ _ __ 
 | |   / _ \ / _` | | '_ \ ____\___ \ / _ \ '__\ \ / / _ \ '__|
 | |__| (_) | (_| | | | | |_____|__) |  __/ |   \ V /  __/ |   
 |_____\___/ \__, |_|_| |_|    |____/ \___|_|    \_/ \___|_|   
             |___/                                             
";
            string separator = new('=', Console.WindowWidth);
            string logo = text.Split('\n').Select(s => string.Format("{0," + (Console.WindowWidth / 2 + s.Length / 2) + "}\n", s))
                .Aggregate("", (current, i) => current + i);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(separator + logo + $"Version: {Assembly.GetExecutingAssembly().GetName().Version}\n" + separator);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static async Task Main(string[] args)
        {
            // workaround
            // http2 needs SSL
            // https://github.com/grpc/grpc-dotnet/issues/626
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var stopService = new DockerGracefulStopService();
            PrintHeader();

            using IHost host = CreateHostBuilder(args).Build();
            {
                await host.StartAsync();

                using (IServiceScope scope = host.Services.CreateScope())
                {
                    IServiceProvider services = scope.ServiceProvider;

                    IGlobalPacketProcessor processor = services.GetRequiredService<IGlobalPacketProcessor>();
                    processor.RegisterHandler(typeof(Nos0575Packet), services.GetRequiredService<TypedCredentialsLoginPacketHandler>());
                    processor.RegisterHandler(typeof(Nos0577Packet), services.GetRequiredService<NewLoginPacketHandler>());

                    IServerApiService master = services.GetRequiredService<IServerApiService>();

                    BasicRpcResponse response = null;
                    while (response == null)
                    {
                        try
                        {
                            response = await master.IsMasterOnline(new());
                        }
                        catch
                        {
                            Log.Warn("Failed to contact with Master Server, retrying in 2 seconds...");
                            await Task.Delay(TimeSpan.FromSeconds(2));
                        }
                    }

                    if (response.ResponseType != RpcResponseType.SUCCESS)
                    {
                        Log.Warn("Master Server has refused the registration of this Login Server");
                        return;
                    }

                    Log.Info("Login Server has been registered successfully");

                    IPacketDeserializer packetDeserializer = services.GetRequiredService<IPacketDeserializer>();

                    int port = Convert.ToInt32(Environment.GetEnvironmentVariable("SERVER_PORT") ?? "4000");
                    Network.LoginServer server;
                    try
                    {
                        server = new Network.LoginServer(IPAddress.Any, port, new SmartSpamProtector(), processor, packetDeserializer);
                        server.Start();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("General Error Server", ex);
                        await host.StopAsync();
                        return;
                    }

                    IMessagingService messagingService = services.GetService<IMessagingService>();
                    if (messagingService != null)
                    {
                        await messagingService.StartAsync();
                    }

                    Log.Info("Game Authentication service online!");
                    await host.WaitForShutdownAsync(stopService.CancellationToken);

                    if (messagingService != null)
                    {
                        await messagingService.DisposeAsync();
                    }

                    server?.Stop();
                }
            }
        }



        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel();
                });
            return builder;
        }
    }
}