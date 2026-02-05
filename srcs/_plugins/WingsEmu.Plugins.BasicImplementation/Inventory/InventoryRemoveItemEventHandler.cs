using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryRemoveItemEventHandler : IAsyncEventProcessor<InventoryRemoveItemEvent>
{
    public async Task HandleAsync(InventoryRemoveItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        int itemVnum = e.ItemVnum;
        int amount = e.Amount;
        InventoryItem inventoryItem = e.InventoryItem;
        bool sendPackets = e.SendPackets;

        if (inventoryItem == null) // find item by vnum
        {
            if (!session.PlayerEntity.HasItem(itemVnum, amount))
            {
                return;
            }

            int toRemove = amount;
            foreach (InventoryItem invItem in session.PlayerEntity.GetAllPlayerInventoryItems())
            {
                if (toRemove <= 0)
                {
                    break;
                }

                GameItemInstance item = invItem?.ItemInstance;
                if (item == null)
                {
                    continue;
                }

                if (item.GameItem.Id != itemVnum)
                {
                    continue;
                }

                if (item.Amount - toRemove <= 0)
                {
                    toRemove -= item.Amount;
                    item.Amount = 0;
                }
                else
                {
                    item.Amount -= toRemove;
                    toRemove -= amount;
                }

                if (item.Amount > 0)
                {
                    if (sendPackets)
                    {
                        session.SendInventoryAddPacket(invItem);
                    }

                    continue;
                }

                if (!session.PlayerEntity.RemoveItemFromSlotAndType(invItem.Slot, invItem.InventoryType, out InventoryItem removedItemAmount))
                {
                    continue;
                }

                await session.EmitEventAsync(new InventoryItemDeletedEvent
                {
                    ItemInstance = removedItemAmount.ItemInstance,
                    ItemAmount = toRemove
                });

                if (sendPackets)
                {
                    session.SendInventoryRemovePacket(removedItemAmount);
                }
            }

            return;
        }

        inventoryItem.ItemInstance.Amount -= amount;
        if (inventoryItem.ItemInstance.Amount > 0)
        {
            if (sendPackets)
            {
                session.SendInventoryAddPacket(inventoryItem);
            }

            return;
        }

        bool shouldResetStats = false;
        if (e.IsEquipped)
        {
            if (e.InventoryItem.ItemInstance.GameItem.EquipmentSlot == EquipmentType.Amulet)
            {
                await session.EmitEventAsync(new InventoryTakeOffItemEvent(e.InventoryItem.Slot)
                {
                    ForceToRandomSlot = true
                });
                shouldResetStats = true;
            }

            if (e.InventoryItem.ItemInstance.GameItem.EquipmentSlot != EquipmentType.Amulet
                && (e.InventoryItem.ItemInstance.ItemDeleteTime == null || e.InventoryItem.ItemInstance.ItemDeleteTime < DateTime.UtcNow))
            {
                await session.EmitEventAsync(new InventoryTakeOffItemEvent(e.InventoryItem.Slot)
                {
                    ForceToRandomSlot = true
                });
                shouldResetStats = true;
            }
        }

        if (!session.PlayerEntity.RemoveItemFromSlotAndType(inventoryItem.Slot, inventoryItem.InventoryType, out InventoryItem itemBySlot))
        {
            return;
        }

        await session.EmitEventAsync(new InventoryItemDeletedEvent
        {
            ItemInstance = itemBySlot.ItemInstance,
            ItemAmount = amount
        });

        if (sendPackets)
        {
            session.SendInventoryRemovePacket(itemBySlot);
        }

        if (!e.IsEquipped && !shouldResetStats)
        {
            return;
        }

        if (!sendPackets)
        {
            return;
        }

        session.SendEmptyAmuletBuffPacket();
        session.RefreshStatChar();
    }
}