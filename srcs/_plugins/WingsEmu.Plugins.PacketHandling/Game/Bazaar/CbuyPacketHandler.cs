using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CbuyPacketHandler : GenericGamePacketHandlerBase<CBuyPacket>
{
    private readonly BazaarConfiguration _bazaarConfiguration;

    public CbuyPacketHandler(BazaarConfiguration bazaarConfiguration) => _bazaarConfiguration = bazaarConfiguration;

    protected override async Task HandlePacketAsync(IClientSession session, CBuyPacket cBuyPacket)
    {
        DateTime currentDate = DateTime.UtcNow;
        if (session.PlayerEntity.LastBuyBazaarRefresh > currentDate)
        {
            return;
        }

        session.PlayerEntity.LastBuyBazaarRefresh = currentDate.AddSeconds(_bazaarConfiguration.DelayServerBetweenRequestsInSecs);

        if (BazaarExtensions.PriceOrAmountExceeds(true, cBuyPacket.Price, cBuyPacket.Amount))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Exceeding the price or amount limit");
            return;
        }

        await session.EmitEventAsync(new BazaarItemBuyEvent(cBuyPacket.BazaarItemId, cBuyPacket.Amount, cBuyPacket.Price));
    }
}