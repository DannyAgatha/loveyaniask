using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.Raids;

public class RevivalEventRaidHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IBuffFactory _buffFactory;

    public RevivalEventRaidHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.PlayerEntity.IsAlive() || session.CurrentMapInstance is not { MapInstanceType: MapInstanceType.RaidInstance })
        {
            return;
        }

        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
        session.RefreshStat();
        await session.PlayerEntity.Restore(restoreMates: false);
        session.BroadcastRevive();
        await session.CheckPartnerBuff();
        session.SendBuffsPacket();
    }
}