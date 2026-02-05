using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act4.Event;

public class PortalTriggerAct4EventHandler : IAsyncEventProcessor<PortalTriggerEvent>
{
    private readonly Act4Configuration _act4Configuration;
    private readonly IGameLanguageService _languageService;
    private readonly IMapManager _mapManager;
    private readonly IAct4CaligorManager _act4CaligorManager;

    public PortalTriggerAct4EventHandler(Act4Configuration act4Configuration, IGameLanguageService languageService, IMapManager mapManager, IAct4CaligorManager act4CaligorManager)
    {
        _act4Configuration = act4Configuration;
        _languageService = languageService;
        _mapManager = mapManager;
        _act4CaligorManager = act4CaligorManager;
    }

    public async Task HandleAsync(PortalTriggerEvent e, CancellationToken cancellation)
    {
        if (!e.Sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (e.Sender.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        List<int> bannedMapIds = e.Sender.PlayerEntity.Faction == FactionType.Angel ? _act4Configuration.BannedMapIdsToAngels : _act4Configuration.BannedMapIdsToDemons;
        if (e.Portal.DestinationMapInstance != null && bannedMapIds.Contains(e.Portal.DestinationMapInstance.MapId))
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_BLOCKED, e.Sender.UserLanguage));
            return;
        }

        e.Sender.PlayerEntity.LastPortal = DateTime.UtcNow;
        
        switch (e.Portal.MapInstance.MapVnum)
        {
            case (short)MapIds.UNKNOWN_LAND when e.Portal.DestinationMapInstance is { MapVnum: (short)MapIds.ACT4_CALIGOR }:
            {
                if (_act4CaligorManager.HasLeftCaligor(e.Sender.PlayerEntity))
                {
                    e.Sender.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ACT4_CALIGOR_CANNOT_REENTER, e.Sender.UserLanguage), ChatMessageColorType.Red);
                    e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.ACT4_CALIGOR_CANNOT_REENTER, e.Sender.UserLanguage));
                    return;
                }
                
                AssignFactionsToPlayer(e.Sender);

                short x = e.Sender.PlayerEntity.CaligorFaction == FactionType.Angel ? (short)54 : (short)127;
                short y = e.Sender.PlayerEntity.CaligorFaction == FactionType.Angel ? (short)161 : (short)162;

                e.Sender.ChangeMap(e.Portal.DestinationMapInstance, x, y);
                return;
            }
            
            case (short)MapIds.ACT4_CALIGOR when e.Portal.PositionX == 50 && e.Portal.PositionY == 174 && e.Sender.PlayerEntity.Faction == FactionType.Demon:
            case (short)MapIds.UNKNOWN_LAND when e.Portal.PositionX == 70 && e.Portal.PositionY == 159 && e.Sender.PlayerEntity.Faction == FactionType.Demon:
            case (short)MapIds.ACT4_CALIGOR when e.Portal.PositionX == 128 && e.Portal.PositionY == 175 && e.Sender.PlayerEntity.Faction == FactionType.Angel:
            case (short)MapIds.UNKNOWN_LAND when e.Portal.PositionX == 110 && e.Portal.PositionY == 159 && e.Sender.PlayerEntity.Faction == FactionType.Angel:
                e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_BLOCKED, e.Sender.UserLanguage));
                return;
        }

        switch (e.Sender.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.Caligor:
                if (e.Sender.CurrentMapInstance.MapId == (short)MapIds.ACT4_CALIGOR)
                {
                    if (e.Confirmed)
                    {
                        FactionType originalFaction = _act4CaligorManager.GetInitialFaction(e.Sender.PlayerEntity);
                        
                        e.Sender.EmitEventAsync(new ChangeFactionEvent
                        {
                            NewFaction = originalFaction
                        }).ConfigureAwait(false).GetAwaiter().GetResult();
                        
                        if (e.Sender.PlayerEntity.CaligorFaction.HasValue)
                        {
                            _act4CaligorManager.DecreaseFactionCount(e.Sender.PlayerEntity.CaligorFaction.Value);
                        }
                        
                        e.Sender.PlayerEntity.CaligorFaction = null;
                        _act4CaligorManager.MarkPlayerAsLeftCaligor(e.Sender.PlayerEntity);
                        
                        short x = originalFaction == FactionType.Angel ? (short)70 : (short)110;
                        short y = 159;
                        
                        e.Sender.ChangeMap((short)MapIds.UNKNOWN_LAND, x, y);
                        
                        e.Sender.RefreshFaction();
                    }
                    else
                    {
                        e.Sender.SendQnaPacket("preq 1", _languageService.GetLanguage(GameDialogKey.ACT4_CALIGOR_CONFIRM_LEAVE, e.Sender.UserLanguage));
                    }

                    return;
                }

                break;

        }

        if (e.Portal.Type is PortalType.AngelRaid or PortalType.DemonRaid)
        {
            await e.Sender.EmitEventAsync(new Act4DungeonEnterEvent
            {
                Confirmed = e.Confirmed
            });
            return;
        }

        if (e.Portal.DestinationMapInstance == null)
        {
            return;
        }

        if (e.Portal.Type == PortalType.Closed)
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_BLOCKED, e.Sender.UserLanguage));
            return;
        }
        
        if (e.Portal.PositionX == e.Portal.DestinationX && e.Portal.PositionY == e.Portal.DestinationY)
        {
            if (e.Sender.PlayerEntity.Faction == FactionType.Angel)
            {
                e.Sender.ChangeMap((short)MapIds.UNKNOWN_LAND, 46, 171);
                return;
            }
            e.Sender.ChangeMap((short)MapIds.UNKNOWN_LAND, 135, 171);
            return;
        }

        //TP Logic
        if (e.Portal.DestinationX == -1 && e.Portal.DestinationY == -1)
        {
            await _mapManager.TeleportOnRandomPlaceInMapAsync(e.Sender, e.Portal.DestinationMapInstance);
            return;
        }
        
        if (e.Portal.DestinationMapInstance?.Id == e.Sender.PlayerEntity.MapInstanceId && e.Portal.DestinationX.HasValue && e.Portal.DestinationY.HasValue)
        {
            e.Sender.PlayerEntity.TeleportOnMap(e.Portal.DestinationX.Value, e.Portal.DestinationY.Value, true);
            return;
        }

        e.Sender.ChangeMap(e.Portal.DestinationMapInstance, e.Portal.DestinationX, e.Portal.DestinationY);
    }
    
    private void AssignFactionsToPlayer(IClientSession session)
    {
        if (session.PlayerEntity.CaligorFaction != null)
        {
            return;
        }

        int angelCount = _act4CaligorManager.AngelCount;
        int demonCount = _act4CaligorManager.DemonCount;

        if (angelCount > demonCount)
        {
            session.PlayerEntity.CaligorFaction = FactionType.Demon;
            _act4CaligorManager.UpdateFactionCount(FactionType.Demon);
        }
        else
        {
            session.PlayerEntity.CaligorFaction = FactionType.Angel;
            _act4CaligorManager.UpdateFactionCount(FactionType.Angel);
        }

        session.EmitEventAsync(new ChangeFactionEvent
        {
            NewFaction = session.PlayerEntity.CaligorFaction.Value
        }).ConfigureAwait(false).GetAwaiter().GetResult();
    }

}