using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Ship.Event;

public class ShipProcessEventAct4Handler : IAsyncEventProcessor<ShipProcessEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IServerApiService _serverApiService;
    private readonly IServerManager _serverManager;

    public ShipProcessEventAct4Handler(IServerApiService serverApiService, IServerManager serverManager, IGameLanguageService languageService, IRandomGenerator randomGenerator)
    {
        _serverApiService = serverApiService;
        _serverManager = serverManager;
        _languageService = languageService;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(ShipProcessEvent e, CancellationToken cancellation)
    {
        if (e.ShipInstance.ShipType != ShipType.Act4Angels && e.ShipInstance.ShipType != ShipType.Act4Demons)
        {
            return;
        }

        await ProcessDeparture(e.ShipInstance, e.CurrentTime);
    }

    private async Task ProcessDeparture(ShipInstance shipInstance, DateTime currentTime)
    {
        if (currentTime < shipInstance.LastDeparture + shipInstance.Configuration.Departure)
        {
            return;
        }

        GetChannelInfoResponse response = null;
        try
        {
            response = await _serverApiService.GetAct4ChannelInfo(new GetAct4ChannelInfoRequest
            {
                WorldGroup = _serverManager.ServerGroup
            });
        }
        catch (Exception e)
        {
            Log.Error("[SHIP_PROCESS_ACT4] Unexpected error happened while trying to obtain Act4 Channel's Info.", e);
        }

        SerializableGameServer gameServer = response?.GameServer;

        if (response?.ResponseType != RpcResponseType.SUCCESS || gameServer == null)
        {
            shipInstance.MapInstance.Broadcast(x => x.GenerateMsgPacket(_languageService.GetLanguage(GameDialogKey.ACT4_CHANNEL_OFFLINE, x.UserLanguage), MsgMessageType.Middle));
            return;
        }

        foreach (IClientSession session in shipInstance.MapInstance.Sessions.ToList())
        {
            long baseToRemove = shipInstance.Configuration.ShipCost;
            short toRemove = session.PlayerEntity.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.DECREASE_SHIP_TP_COST).Item1 ?? 0;
            long amountToRemove = (long)(baseToRemove * (toRemove * 0.01));
            baseToRemove -= amountToRemove;

            if (!session.PlayerEntity.RemoveGold(baseToRemove))
            {
                session.SendErrorChatMessage(_languageService.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage));
                session.ChangeToLastBaseMap();
                continue;
            }
            
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, baseToRemove));
            await session.EmitEventAsync(new PlayerChangeChannelEvent(gameServer, ItModeType.ShipToAct4, (short)shipInstance.Configuration.DestinationMapId,
                (short)_randomGenerator.RandomNumber(shipInstance.Configuration.DestinationMapX.Minimum, shipInstance.Configuration.DestinationMapX.Maximum + 1),
                (short)_randomGenerator.RandomNumber(shipInstance.Configuration.DestinationMapY.Minimum, shipInstance.Configuration.DestinationMapY.Maximum + 1)));
        }

        shipInstance.DepartureWarnings = shipInstance.Configuration.DepartureWarnings.ToList();
        shipInstance.LastDeparture = currentTime;
    }
}