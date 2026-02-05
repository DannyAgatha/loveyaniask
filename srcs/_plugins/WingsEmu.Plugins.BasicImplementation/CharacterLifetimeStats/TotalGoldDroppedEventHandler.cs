using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalGoldDroppedEventHandler : IAsyncEventProcessor<InventoryPickedUpItemEvent>
{
    public async Task HandleAsync(InventoryPickedUpItemEvent e, CancellationToken cancellation)
    {
        if (e.ItemVnum != (int)ItemVnums.GOLD)
        {
            return;
        }

        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalGoldDropped += e.Amount;
    }
}