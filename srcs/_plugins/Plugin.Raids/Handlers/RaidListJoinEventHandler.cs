using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidListJoinEventHandler : IAsyncEventProcessor<RaidListJoinEvent>
{
    private readonly IRaidManager _raidManager;
    private readonly ISessionManager _sessionManager;

    public RaidListJoinEventHandler(ISessionManager sessionManager, IRaidManager raidManager)
    {
        _sessionManager = sessionManager;
        _raidManager = raidManager;
    }

    public async Task HandleAsync(RaidListJoinEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        string nickname = e.Nickname;

        if (string.IsNullOrEmpty(nickname))
        {
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.HasRaidStarted)
        {
            return;
        }

        IClientSession leader = _sessionManager.GetSessionByCharacterName(nickname);
        if (leader == null)
        {
            return;
        }

        if (!leader.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (!leader.PlayerEntity.IsRaidLeader(leader.PlayerEntity.Id))
        {
            return;
        }

        RaidParty leaderRaid = leader.PlayerEntity.Raid;
        if (!_raidManager.ContainsRaidInRaidPublishList(leaderRaid))
        {
            return;
        }

        await session.EmitEventAsync(new RaidPartyJoinEvent(leader.PlayerEntity.Id, true));
    }
}