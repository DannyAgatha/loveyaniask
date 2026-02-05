using PhoenixLib.Events;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorRefreshScoreEventHandler : IAsyncEventProcessor<AlzanorRefreshScoreEvent>
{
    
    private readonly IAlzanorManager _alzanorManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    
    public AlzanorRefreshScoreEventHandler(IAlzanorManager alzanorManager, IAsyncEventPipeline asyncEventPipeline)
    {
        _alzanorManager = alzanorManager;
        _asyncEventPipeline = asyncEventPipeline;
    }


    public async Task HandleAsync(AlzanorRefreshScoreEvent e, CancellationToken cancellation)
    {
        AlzanorParty alzanorParty = e.AlzanorParty;
        if (alzanorParty == null)
        {
            return;
        }
        if (!alzanorParty.Started)
        {
            return;
        }
        if (alzanorParty.FinishTime != null)
        {
            return;
        }
        
        if (!_alzanorManager.IsActive)
        {
            return;
        }
        
        DateTime currentTime = DateTime.UtcNow;

        if (currentTime >= _alzanorManager.AlzanorEnd)
        {
            await _asyncEventPipeline.ProcessEventAsync(new AlzanorEndEvent(), cancellation);
            return;
        }

        IMapInstance alzanorMap = _alzanorManager.GetAlzanorInstance();
        if(alzanorMap == null)
        {
            return;
        }
        
        if (!alzanorMap.Sessions.Any())
        {
            return;
        }
        
        _alzanorManager.RefreshAlzanorInstance();
    }
}