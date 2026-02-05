using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event;

public class PortalTriggerDungeonEventHandler : IAsyncEventProcessor<PortalTriggerEvent>
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IMapManager _mapManager;

    public PortalTriggerDungeonEventHandler(IMapManager mapManager, Act4DungeonsConfiguration act4DungeonsConfiguration)
    {
        _mapManager = mapManager;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
    }

    public async Task HandleAsync(PortalTriggerEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Dungeon)
        {
            return;
        }

        switch (e.Portal.Type)
        {
            case PortalType.TSNormal:
                if (!e.Confirmed && e.Portal.DestinationMapInstance == null) // Tundra portal
                {
                    e.Sender.SendQnaPacket("preq 1", e.Sender.GetLanguage(GameDialogKey.ACT4_ASK_DIALOG_DUNGEON_EXIT));
                    return;
                }

                break;
            case PortalType.Open:
                break;
            default:
                return;
        }

        e.Sender.PlayerEntity.LastPortal = DateTime.UtcNow;

        if (e.Portal.DestinationMapInstance == null)
        {
            e.Sender.ChangeMap(_act4DungeonsConfiguration.DungeonReturnPortalMapId, _act4DungeonsConfiguration.DungeonReturnPortalMapX, _act4DungeonsConfiguration.DungeonReturnPortalMapY);
            return;
        }

        await ProcessTeleport(e.Sender, e.Portal);
    }

    private async Task ProcessTeleport(IClientSession session, IPortalEntity portal)
    {
        if (portal.DestinationMapInstance == null)
        {
            return;
        }

        if (portal.DestinationX == -1 && portal.DestinationY == -1)
        {
            await _mapManager.TeleportOnRandomPlaceInMapAsync(session, portal.DestinationMapInstance);
            return;
        }

        if (portal.DestinationMapInstance.Id == session.PlayerEntity.MapInstanceId && portal.DestinationX.HasValue && portal.DestinationY.HasValue)
        {
            session.PlayerEntity.TeleportOnMap(portal.DestinationX.Value, portal.DestinationY.Value, true);
            return;
        }

        session.ChangeMap(portal.DestinationMapInstance, portal.DestinationX, portal.DestinationY);
    }
}