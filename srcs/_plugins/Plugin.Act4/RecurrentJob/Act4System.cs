using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.RecurrentJob;

public class Act4System : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private readonly IAct4Manager _act4Manager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly ISessionManager _sessionManager;

    public Act4System(IAct4Manager act4Manager, ISessionManager sessionManager, IAsyncEventPipeline asyncEventPipeline)
    {
        _act4Manager = act4Manager;
        _sessionManager = sessionManager;
        _asyncEventPipeline = asyncEventPipeline;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[ACT4_SYSTEM] Started!");
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessAct4(stoppingToken);
            await ProcessAct4Mukraju(stoppingToken);
            await _asyncEventPipeline.ProcessEventAsync(new Act4SystemFcBroadcastEvent(), stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessAct4(CancellationToken stoppingToken)
    {
        if (!_act4Manager.FactionPointsLocked)
        {
            await _asyncEventPipeline.ProcessEventAsync(new Act4FactionPointsGenerationEvent(), stoppingToken);
        }
    }

    private async Task ProcessAct4Mukraju(CancellationToken stoppingToken)
    {
        FactionType faction = _act4Manager.GetTriumphantFaction();
        if (faction == FactionType.Neutral)
        {
            (DateTime deleteTime, IMonsterEntity mukraju, FactionType _) = _act4Manager.GetMukraju();
            if (DateTime.UtcNow < deleteTime || mukraju == null)
            {
                return;
            }

            await _asyncEventPipeline.ProcessEventAsync(new Act4MukrajuDespawnEvent(), stoppingToken);
            return;
        }

        await _asyncEventPipeline.ProcessEventAsync(new Act4MukrajuSpawnEvent(faction), stoppingToken);
    }
}