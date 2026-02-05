using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class CardNameHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 668 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsAvailableToChangeName)
        {
            return;
        }
        
        session.PlayerEntity.IsAvailableToChangeName = true;
        await session.RemoveItemFromInventory(item: e.Item);

        session.ForceDisconnect();
    }
}