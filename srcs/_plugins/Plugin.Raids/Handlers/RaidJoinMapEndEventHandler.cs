using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;

namespace Plugin.Raids.Handlers;

public class RaidJoinMapEndEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
{
    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.RaidInstance || e.Sender.PlayerEntity.Raid == null)
        {
            return;
        }

        e.Sender.SendTsClockPacket(e.Sender.PlayerEntity.Raid.Instance.TimeUntilEnd, true);
        e.Sender.SendRaidmbf();
        e.Sender.SendRaidPacket(RaidPacketType.REFRESH_MEMBERS_HP_MP);
        foreach (IClientSession member in e.Sender.PlayerEntity.Raid.Members)
        {
            e.Sender.SendStPacket(member.PlayerEntity);
        }
    }
}