using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidPartyKickPlayerEventHandler : IAsyncEventProcessor<RaidPartyKickPlayerEvent>
{
    private readonly ISessionManager _sessionManager;

    public RaidPartyKickPlayerEventHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(RaidPartyKickPlayerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long targetId = e.CharacterId;

        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.Id == targetId)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        if (session.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        IClientSession target = _sessionManager.GetSessionByCharacterId(targetId);
        if (target == null)
        {
            return;
        }

        if (!target.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (target.PlayerEntity.Raid.Id != session.PlayerEntity.Raid.Id)
        {
            return;
        }

        await target.EmitEventAsync(new RaidPartyLeaveEvent(true));
    }
}