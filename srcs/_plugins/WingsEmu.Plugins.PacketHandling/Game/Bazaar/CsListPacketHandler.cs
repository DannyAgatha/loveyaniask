using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CsListPacketHandler : GenericGamePacketHandlerBase<CsListPacket>
{
    private readonly BazaarConfiguration _bazaarConfiguration;

    public CsListPacketHandler(BazaarConfiguration bazaarConfiguration) => _bazaarConfiguration = bazaarConfiguration;

    protected override async Task HandlePacketAsync(IClientSession session, CsListPacket packet)
    {
        if (!Enum.IsDefined(typeof(BazaarListedItemType), packet.Filter))
        {
            return;
        }

        DateTime currentDate = DateTime.UtcNow;
        if (session.PlayerEntity.LastAdministrationBazaarRefresh > currentDate)
        {
            return;
        }

        session.PlayerEntity.LastAdministrationBazaarRefresh = currentDate.AddSeconds(_bazaarConfiguration.DelayClientBetweenRequestsInSecs);

        await session.EmitEventAsync(new BazaarGetListedItemsEvent(packet.Index, (BazaarListedItemType)packet.Filter));
    }
}