using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.PrivateMapInstances;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.PrivateMapInstance;

public class PrivateMapJoinEndEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
{
    private readonly IPrivateMapInstanceManager _privateMapInstanceManager;

    public PrivateMapJoinEndEventHandler(IPrivateMapInstanceManager privateMapInstanceManager)
    {
        _privateMapInstanceManager = privateMapInstanceManager;
    }

    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        IMapInstance mapInstance = e.JoinedMapInstance;
        IClientSession session = e.Sender;
        if (mapInstance is not { MapInstanceType: MapInstanceType.PrivateInstance })
        {
            return;
        }

        WingsEmu.Game.PrivateMapInstances.PrivateMapInstance privateMapInstance = _privateMapInstanceManager.GetByMapVnum(mapInstance.MapVnum);
        if (privateMapInstance is null)
        {
            return;
        }
        
        session.SendChatMessage(session.GetLanguage(GameDialogKey.PRIVATE_MAP_INSTANCE_CHATMESSAGE_JOINED_TO_MAP), ChatMessageColorType.Yellow);
        session.PlayerEntity.PrivateMapInstanceInfo = new PrivateMapInstanceInfo();
        session.SendTsClockPacket(privateMapInstance.EndTime - DateTime.UtcNow, true);
    }
}