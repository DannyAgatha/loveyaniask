using PhoenixLib.Events;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorDestroyEventHandler : IAsyncEventProcessor<AlzanorDestroyEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IAlzanorManager _alzanorManager;

    public AlzanorDestroyEventHandler(IMapManager mapManager, IAlzanorManager alzanorManager)
    {
        _mapManager = mapManager;
        _alzanorManager = alzanorManager;
    }

    public async Task HandleAsync(AlzanorDestroyEvent e, CancellationToken cancellation)
    {
        AlzanorParty alzanorParty = e.AlzanorParty;
        _alzanorManager.RemoveAlzanor(alzanorParty);

        IMapInstance mapInstance = alzanorParty.MapInstance;
        foreach (IClientSession session in mapInstance.Sessions)
        {
            await session.EmitEventAsync(new AlzanorLeaveEvent
            {
                AddLeaverBuster = false
            });
        }

        _alzanorManager.ClearEverything();
        _mapManager.RemoveMapInstance(mapInstance.Id);
    }
}