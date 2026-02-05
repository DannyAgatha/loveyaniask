using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidListOpenEventHandler : IAsyncEventProcessor<RaidListOpenEvent>
{
    private readonly IRaidManager _raidManager;

    public RaidListOpenEventHandler(IRaidManager raidManager) => _raidManager = raidManager;

    public async Task HandleAsync(RaidListOpenEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        byte rlType = 0;

        if (session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            RaidParty raid = session.PlayerEntity.Raid;
            rlType = !_raidManager.ContainsRaidInRaidPublishList(raid) ? (byte)2 : (byte)1;
        }

        session.SendRlPacket(rlType, _raidManager);
    }
}