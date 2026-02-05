using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class PlayerChangeChannelEventHandler : IAsyncEventProcessor<PlayerChangeChannelEvent>
{
    private readonly ISessionService _sessionService;

    public PlayerChangeChannelEventHandler(ISessionService sessionService) => _sessionService = sessionService;

    public async Task HandleAsync(PlayerChangeChannelEvent e, CancellationToken cancellation)
    {
        SessionResponse result = await _sessionService.ActivateCrossChannelAuthentication(new ActivateCrossChannelAuthenticationRequest
        {
            AccountId = e.Sender.Account.Id,
            ChannelId = e.GameServer.ChannelId
        });

        if (result.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn("[CROSS_AUTH] Failure");
            return;
        }

        e.Sender.PlayerEntity.MapId = e.MapId;
        e.Sender.PlayerEntity.MapX = e.MapX;
        e.Sender.PlayerEntity.MapY = e.MapY;

        e.Sender.SendMzPacket(e.GameServer.EndPointIp, (short)e.GameServer.EndPointPort);
        e.Sender.SendItPacket(e.ModeType);

        e.Sender.ForceDisconnect();
    }
}