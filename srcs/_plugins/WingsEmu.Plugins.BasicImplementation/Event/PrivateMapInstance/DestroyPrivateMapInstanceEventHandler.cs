using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Game.PrivateMapInstances.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.PrivateMapInstance;

public class DestroyPrivateMapInstanceEventHandler : IAsyncEventProcessor<DestroyPrivateMapInstanceEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IPrivateMapInstanceManager _privateMapInstanceManager;

    public DestroyPrivateMapInstanceEventHandler(IMapManager mapManager, IPrivateMapInstanceManager privateMapInstanceManager)
    {
        _mapManager = mapManager;
        _privateMapInstanceManager = privateMapInstanceManager;
    }

    public async Task HandleAsync(DestroyPrivateMapInstanceEvent e, CancellationToken cancellation)
    {
        WingsEmu.Game.PrivateMapInstances.PrivateMapInstance privateMapInstance = e.PrivateMapInstance;

        if (privateMapInstance?.MapInstance is null)
        {
            return;
        }
        
        _mapManager.RemoveMapInstance(privateMapInstance.MapInstance.Id);

        switch (privateMapInstance.Type)
        {
            case PrivateMapInstanceType.SOLO when privateMapInstance.PlayerId.HasValue:
                _privateMapInstanceManager.RemoveByPlayer(privateMapInstance.PlayerId.Value);
                break;
            case PrivateMapInstanceType.GROUP when privateMapInstance.GroupId.HasValue:
                _privateMapInstanceManager.RemoveByGroup(privateMapInstance.GroupId.Value);
                break;
            case PrivateMapInstanceType.FAMILY when privateMapInstance.FamilyId.HasValue:
                _privateMapInstanceManager.RemoveByFamily(privateMapInstance.FamilyId.Value);
                break;
            case PrivateMapInstanceType.PUBLIC when privateMapInstance.MapVnum.HasValue:
                _privateMapInstanceManager.RemoveByMapVnum(privateMapInstance.MapVnum.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}