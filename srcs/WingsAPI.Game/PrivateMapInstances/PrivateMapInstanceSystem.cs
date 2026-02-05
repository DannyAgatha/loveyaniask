using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game.PrivateMapInstances.Events;

namespace WingsEmu.Game.PrivateMapInstances;

public class PrivateMapInstanceSystem : BackgroundService
{
    private readonly IPrivateMapInstanceManager _privateMapInstanceManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public PrivateMapInstanceSystem(IPrivateMapInstanceManager privateMapInstanceManager, IAsyncEventPipeline asyncEventPipeline)
    {
        _privateMapInstanceManager = privateMapInstanceManager;
        _asyncEventPipeline = asyncEventPipeline;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[PRIVATE_MAP_INSTANCE_SYSTEM] Start...");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime dateTime = DateTime.UtcNow;
            
            foreach (PrivateMapInstance privateMapInstance in _privateMapInstanceManager.Instances)
            {
                if (privateMapInstance?.MapInstance is null)
                {
                    continue;
                }
                
                await Process(privateMapInstance, dateTime);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task Process(PrivateMapInstance privateMapInstance, DateTime now)
    {
        if (privateMapInstance.StartTime + TimeSpan.FromSeconds(5) > now)
        {
            return;
        }
        
        if ((privateMapInstance.MapInstance.Sessions.Count <= 0 && privateMapInstance.Type != PrivateMapInstanceType.PUBLIC) || now > privateMapInstance.EndTime)
        {
            await _asyncEventPipeline.ProcessEventAsync(new DestroyPrivateMapInstanceEvent
            {
                PrivateMapInstance = privateMapInstance
            });
        }
    }
}