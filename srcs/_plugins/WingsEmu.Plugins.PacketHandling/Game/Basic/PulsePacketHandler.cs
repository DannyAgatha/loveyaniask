using System;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class PulsePacketHandler : GenericGamePacketHandlerBase<PulsePacket>
{
    private readonly ISessionService _sessionService;

    public PulsePacketHandler(ISessionService sessionService) => _sessionService = sessionService;

    protected override async Task HandlePacketAsync(IClientSession session, PulsePacket packet)
    {
        // if player used Speed Hack in CE and client sent pulse packet too quickly
        double seconds = (DateTime.UtcNow - session.PlayerEntity.LastPulseTick).TotalSeconds;
        if (seconds < 50 && !session.IsGameMaster())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Player used Speed Hack in Cheat Engine, because client sent pulse packet too quickly");
            session.ForceDisconnect();
            return;
        }

        session.PlayerEntity.LastPulse += 60;
        if (packet.Tick != session.PlayerEntity.LastPulse)
        {
            session.ForceDisconnect();
            return;
        }

        session.PlayerEntity.LastPulseTick = DateTime.UtcNow;

        SessionResponse response = await _sessionService.Pulse(new PulseRequest
        {
            AccountId = session.Account.Id
        });

        if (response.ResponseType != RpcResponseType.SUCCESS)
        {
            session.ForceDisconnect();
        }
    }
}