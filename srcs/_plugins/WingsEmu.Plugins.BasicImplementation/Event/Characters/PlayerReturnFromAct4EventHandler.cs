using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class PlayerReturnFromAct4EventHandler : IAsyncEventProcessor<PlayerReturnFromAct4Event>
{
    private readonly IGameLanguageService _language;
    private readonly IServerApiService _serverApiService;
    private readonly IServerManager _serverManager;
    private readonly ISessionService _sessionService;

    public PlayerReturnFromAct4EventHandler(ISessionService sessionService, IGameLanguageService language, IServerApiService serverApiService, IServerManager serverManager)
    {
        _sessionService = sessionService;
        _language = language;
        _serverApiService = serverApiService;
        _serverManager = serverManager;
    }


    public async Task HandleAsync(PlayerReturnFromAct4Event e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        SessionResponse sessionResponse = await _sessionService.GetSessionByAccountId(new GetSessionByAccountIdRequest
        {
            AccountId = session.Account.Id
        });

        if (sessionResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            return;
        }

        if (sessionResponse.Session.LastChannelId == 0)
        {
            return;
        }

        GetChannelInfoResponse response = await _serverApiService.GetChannelInfo(new GetChannelInfoRequest
        {
            WorldGroup = _serverManager.ServerGroup,
            ChannelId = sessionResponse.Session.LastChannelId
        });

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return;
        }

        await session.EmitEventAsync(new PlayerChangeChannelEvent(response.GameServer, ItModeType.ToPortAlveus, 145, 51, 41));
    }
}