using System;
using Microsoft.Extensions.DependencyInjection;
using NosEmu.Plugins.BasicImplementations.Ship.RecurrentJob;
using PhoenixLib.Configuration;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;

namespace NosEmu.Plugins.BasicImplementations.Ship;

public static class ShipModuleExtensions
{
    public static void AddShipModule(this IServiceCollection services, GameServerLoader gameServer)
    {
        if (gameServer.Type == GameChannelType.ACT_4)
        {
            Log.Debug("Not loading Ships because this is an Act4 channel.");
            return;
        }

        services.AddSingleton<IShipManager, ShipManager>();
        if (!bool.TryParse(Environment.GetEnvironmentVariable("ACT4_SHIP_ACTIVATED") ?? "true", out bool shipActivated) || !shipActivated)
        {
            return;
        }

        services.AddHostedService<ShipSystem>();
        services.AddFileConfiguration<ShipConfiguration>();
        services.AddSingleton<IShipConfigurationProvider, ShipConfigurationProvider>();
    }
}