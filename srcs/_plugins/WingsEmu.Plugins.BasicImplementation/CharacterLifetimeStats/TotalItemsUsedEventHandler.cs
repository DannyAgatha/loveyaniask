using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Core.ItemHandling.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalItemsUsedEventHandler : IAsyncEventProcessor<InventoryUsedItemEvent>
{
    public async Task HandleAsync(InventoryUsedItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        switch (e.Item.ItemInstance.GameItem.ItemType)
        {
            case ItemType.Potion:
                session.PlayerEntity.LifetimeStats.TotalPotionsUsed++;
                break;
            case ItemType.Food:
                session.PlayerEntity.LifetimeStats.TotalFoodUsed++;
                break;
            case ItemType.Snack:
                session.PlayerEntity.LifetimeStats.TotalSnacksUsed++;
                break;
        }

        session.PlayerEntity.LifetimeStats.TotalItemsUsed++;
    }
}