using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidInstanceRefreshInfoEventHandler : IAsyncEventProcessor<RaidInstanceRefreshInfoEvent>
{
    public async Task HandleAsync(RaidInstanceRefreshInfoEvent e, CancellationToken cancellation)
    {
        string raidHealthPacket = null;

        IClientSession[] members = e.RaidParty.Members.ToArray();
        var stPackets = members.Select(test => test.PlayerEntity.GenerateStPacket()).ToList();

        foreach (IClientSession session in members)
        {
            raidHealthPacket ??= session.GenerateRaidPacket(RaidPacketType.REFRESH_MEMBERS_HP_MP);
            session.SendPacket(raidHealthPacket);
            session.TrySendRaidBossPackets();
            session.SendPackets(stPackets);
        }
    }
}