using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;

namespace NosEmu.Plugins.BasicImplementations.Ship.RecurrentJob;

public class ShipSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IMapManager _mapManager;
    private readonly IShipConfigurationProvider _shipConfigurationProvider;
    private readonly IShipManager _shipManager;

    public ShipSystem(IShipConfigurationProvider shipConfiguration, IShipManager shipManager, IMapManager mapManager, IAsyncEventPipeline asyncEventPipeline)
    {
        _shipConfigurationProvider = shipConfiguration;
        _shipManager = shipManager;
        _mapManager = mapManager;
        _asyncEventPipeline = asyncEventPipeline;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[SHIP_SYSTEM] Started!");
        DateTime currentTime = DateTime.UtcNow;
        int count = 0;
        foreach (WingsEmu.Game.Ship.Configuration.Ship ship in _shipConfigurationProvider.GetShips())
        {
            _shipManager.AddShip(new ShipInstance(_mapManager.GenerateMapInstanceByMapId(ship.ShipMapId, MapInstanceType.NormalInstance), ship, currentTime));
            count++;
        }

        Log.Info($"[SHIP_SYSTEM] {count.ToString()} ships loaded!");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Process();
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task Process()
    {
        DateTime currentTime = DateTime.UtcNow;
        IReadOnlyCollection<ShipInstance> ships = _shipManager.GetShips();
        foreach (ShipInstance ship in ships)
        {
            await _asyncEventPipeline.ProcessEventAsync(new ShipProcessEvent(ship, currentTime));
        }
    }
}