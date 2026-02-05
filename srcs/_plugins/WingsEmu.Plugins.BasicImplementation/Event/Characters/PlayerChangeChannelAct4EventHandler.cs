using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class PlayerChangeChannelAct4EventHandler : IAsyncEventProcessor<PlayerChangeChannelAct4Event>
{
    private readonly SerializableGameServer _gameServer;
    private readonly IMapManager _mapManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IServerApiService _serverApiService;
    private readonly IServerManager _serverManager;

    public PlayerChangeChannelAct4EventHandler(IServerApiService serverApiService, IServerManager serverManager, SerializableGameServer gameServer, IRandomGenerator randomGenerator,
        IMapManager mapManager)
    {
        _serverApiService = serverApiService;
        _serverManager = serverManager;
        _gameServer = gameServer;
        _randomGenerator = randomGenerator;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(PlayerChangeChannelAct4Event e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.Faction == FactionType.Neutral)
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
        catch (Exception ex)
        {
            Log.Error("[PLAYER_CHANGE_CHANNEL_ACT4] Unexpected error while trying to obtain channel's Act4 information: ", ex);
        }

        SerializableGameServer gameServer = response?.GameServer;
        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Error("[GetAct4ChannelInfoRequest] Response type is not success.", new Exception());
            return;
        }

        if (gameServer == null)
        {
            Log.Error("[GetAct4ChannelInfoRequest] Game server is null.", new Exception());
            return;
        }

        if (gameServer.ChannelId == _gameServer.ChannelId)
        {
            Log.Error("[GetAct4ChannelInfoRequest] It's the same channel id.", new Exception());
            return;
        }

        short mapId = e.Sender.PlayerEntity.Faction == FactionType.Angel ? (short)MapIds.ACT4_ANGEL_CITADEL : (short)MapIds.ACT4_DEMON_CITADEL;
        short mapX = (short)(12 + _randomGenerator.RandomNumber(-2, 3));
        short mapY = (short)(40 + _randomGenerator.RandomNumber(-2, 3));

        if (_mapManager.HasMapFlagByMapId(e.Sender.PlayerEntity.MapId, MapFlags.ACT_4))
        {
            mapId = (short)e.Sender.PlayerEntity.MapId;
            mapX = e.Sender.PlayerEntity.MapX;
            mapY = e.Sender.PlayerEntity.MapY;
        }

        await e.Sender.EmitEventAsync(new PlayerChangeChannelEvent(gameServer, ItModeType.ToAct4, mapId, mapX, mapY));
    }
}