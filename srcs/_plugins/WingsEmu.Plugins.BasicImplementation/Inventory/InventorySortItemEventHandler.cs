using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventorySortItemEventHandler : IAsyncEventProcessor<InventorySortItemEvent>
{
    public async Task HandleAsync(InventorySortItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryType inventoryType = e.InventoryType;
        bool confirm = e.Confirm;

        if (inventoryType != InventoryType.Etc && inventoryType != InventoryType.Main && inventoryType != InventoryType.Equipment && inventoryType != InventoryType.Warehouse
            && inventoryType != InventoryType.Partner)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        if (session.PlayerEntity.LastInventorySort.AddMinutes(1) > now)
        {
            return;
        }

        if (!confirm)
        {
            session.SendQnaPacket($"isort {(byte)inventoryType} 1", session.GetLanguage(GameDialogKey.INVENTORY_DIALOG_ASK_SORT));
            return;
        }

        session.PlayerEntity.LastInventorySort = now;
        InventoryItem[] items = session.PlayerEntity.GetItemsByInventoryType(inventoryType).OrderBy(x => x?.ItemInstance.ItemVNum).ToArray();
        for (short i = 0; i < items.Length; i++)
        {
            InventoryItem item = items.ElementAt(i);
            if (item == null)
            {
                continue;
            }

            if (item.Slot != i)
            {
                session.SendInventoryRemovePacket(item);
            }

            item.Slot = i;
        }

        session.SendStartStartupInventory();
    }
}