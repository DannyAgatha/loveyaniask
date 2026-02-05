using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations.WorldBoss;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class PortalTriggerEventHandler : IAsyncEventProcessor<PortalTriggerEvent>
{
    private readonly IMapManager _mapManager;
    private readonly IGameLanguageService _languageService;

    public PortalTriggerEventHandler(IMapManager mapManager, IGameLanguageService languageService)
    {
        _mapManager = mapManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(PortalTriggerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        switch (e.Portal.Type)
        {
            case PortalType.MapPortal:
            case PortalType.Open:
            case PortalType.Miniland:
            case PortalType.Exit:
            case PortalType.Effect:
            case PortalType.ShopTeleport:
            case PortalType.TimeSpace:
            case PortalType.TSNormal when e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance &&
                e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.RaidInstance:
                break;
            default:
                return;
        }

        switch (session.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.RaidInstance:
                if (e.Portal.Type != PortalType.Open)
                {
                    break;
                }

                RaidParty raidParty = session.PlayerEntity.Raid;
                if (raidParty == null)
                {
                    break;
                }

                if (e.Portal.DestinationMapInstance != null && raidParty.Leader.CurrentMapInstance.Id == e.Portal.DestinationMapInstance.Id)
                {
                    break;
                }

                if (e.Portal.DestinationMapInstance is { IsBossRoom: true })
                {
                    foreach (IClientSession raidMember in raidParty.Members)
                    {
                        await ProcessTeleport(raidMember, e.Portal);
                    }
                }

                await ProcessTeleport(raidParty.Leader, e.Portal);
                break;
            case MapInstanceType.Miniland:
            case MapInstanceType.ArenaInstance:
            case MapInstanceType.EventGameInstance:
            case MapInstanceType.UnderWaterShowdown:
                session.ChangeToLastBaseMap();
                return;
            case MapInstanceType.PrivateInstance:
                if(e.Confirmed)
                {
                    session.ChangeToLastBaseMap();
                }
                else
                {
                    e.Sender.SendQnaPacket("preq 1", "Are you sure you want to leave the private map instance?");
                }
                return;
            case MapInstanceType.TimeSpaceInstance:
            case MapInstanceType.Act4Instance:
                return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (e.Portal.DestinationMapInstance == null)
        {
            return;
        }

        e.Sender.PlayerEntity.LastPortal = DateTime.UtcNow;
        await ProcessTeleport(session, e.Portal);
    }

    private async Task ProcessTeleport(IClientSession session, IPortalEntity portal)
    {
        if (portal.LevelRequired.HasValue && session.PlayerEntity.Level < portal.LevelRequired.Value)
        {
            session.SendChatMessage(_languageService.GetLanguageFormat(GameDialogKey.PORTAL_LEVEL_REQUIRED_MESSAGE, session.UserLanguage, portal.LevelRequired.Value), ChatMessageColorType.Yellow);
            session.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.PORTAL_LEVEL_REQUIRED_MESSAGE, session.UserLanguage, portal.LevelRequired.Value));
            return;
        }
        
        if (portal.HeroLevelRequired.HasValue && session.PlayerEntity.HeroLevel < portal.HeroLevelRequired.Value)
        {
            session.SendChatMessage(_languageService.GetLanguageFormat(GameDialogKey.PORTAL_HERO_LEVEL_REQUIRED_MESSAGE, session.UserLanguage, portal.HeroLevelRequired.Value), ChatMessageColorType.Yellow);
            session.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.PORTAL_HERO_LEVEL_REQUIRED_MESSAGE, session.UserLanguage, portal.HeroLevelRequired.Value));
            return;
        }
        
        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && portal.DestinationMapInstance != null && portal.DestinationMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.ChangeToLastBaseMap();
            return;
        }

        if (portal.DestinationMapInstance?.Id == session.PlayerEntity.Miniland.Id)
        {
            session.ChangeMap(session.PlayerEntity.Miniland, portal.DestinationX, portal.DestinationY);
            return;
        }

        if (portal.DestinationX == -1 && portal.DestinationY == -1)
        {
            await _mapManager.TeleportOnRandomPlaceInMapAsync(session, portal.DestinationMapInstance);
            return;
        }

        if (portal.DestinationMapInstance?.Id == session.PlayerEntity.MapInstanceId && portal.DestinationX.HasValue && portal.DestinationY.HasValue)
        {
            session.PlayerEntity.TeleportOnMap(portal.DestinationX.Value, portal.DestinationY.Value, true);
            return;
        }

        session.ChangeMap(portal.DestinationMapInstance, portal.DestinationX, portal.DestinationY);
    }
}