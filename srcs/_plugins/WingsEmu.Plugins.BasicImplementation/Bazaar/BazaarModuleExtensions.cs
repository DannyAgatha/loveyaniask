using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Configuration;
using WingsEmu.Communication.gRPC.Extensions;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public static class BazaarModuleExtensions
{
    public static void AddBazaarModule(this IServiceCollection services)
    {
        services.AddGrpcBazaarServiceClient();

        services.AddFileConfiguration<BazaarConfiguration>();
        services.AddSingleton<IBazaarManager, BazaarManager>();
    }
}