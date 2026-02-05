using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryMoveItemEventHandler : IAsyncEventProcessor<InventoryMoveItemEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public InventoryMoveItemEventHandler(IGameItemInstanceFactory gameItemInstance, GeneralServerConfiguration generalServerConfiguration)
    {
        _gameItemInstance = gameItemInstance;
        _generalServerConfiguration = generalServerConfiguration;
    }

    public async Task HandleAsync(InventoryMoveItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryType inventoryType = e.InventoryType;
        short slot = e.Slot;
        int amount = e.Amount;
        bool sendPackets = e.SendPackets;

        InventoryType destinationType = e.DestinationType;
        short destinationSlot = e.DestinationSlot;

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
        if (item == null)
        {
            return;
        }

        if (amount > item.ItemInstance.Amount)
        {
            return;
        }

        InventoryItem anotherItem = session.PlayerEntity.GetItemBySlotAndType(destinationSlot, destinationType);
        if (anotherItem == null)
        {
            GameItemInstance itemCopy = _gameItemInstance.DuplicateItem(item.ItemInstance);
            if (item.ItemInstance.Amount != amount)
            {
                if (!session.PlayerEntity.HasSpaceFor(item.ItemInstance.ItemVNum, amount))
                {
                    return;
                }

                await session.RemoveItemFromInventory(amount: amount, item: item);
                GameItemInstance sameItem = itemCopy;
                sameItem.Amount = amount;
                await session.AddNewItemToInventory(sameItem, slot: destinationSlot, isByMovePacket: true);
                return;
            }

            if (item.InventoryType == InventoryType.EquippedItems && item.IsEquipped)
            {
                return;
            }

            if (inventoryType != destinationType)
            {
                if (!session.PlayerEntity.HaveSlotInSpecialInventoryType(destinationType))
                {
                    return;
                }

                await session.RemoveItemFromInventory(amount: amount, item: item);
                await session.AddNewItemToInventory(itemCopy, slot: destinationSlot, type: destinationType);
                return;
            }

            if (sendPackets)
            {
                session.SendInventoryRemovePacket(item);
            }

            item.Slot = destinationSlot;

            if (sendPackets)
            {
                session.SendInventoryAddPacket(item);
            }

            return;
        }

        if (item.ItemInstance.ItemVNum == anotherItem.ItemInstance.ItemVNum && !item.ItemInstance.GameItem.IsNotStackableInventoryType())
        {
            int itemAmount = amount;
            int anotherItemAmount = anotherItem.ItemInstance.Amount;
            if (itemAmount + anotherItemAmount > _generalServerConfiguration.MaxItemAmount) // configure if we wanna increase amount stack
            {
                itemAmount = (short)(_generalServerConfiguration.MaxItemAmount - anotherItemAmount);
                if (itemAmount <= 0)
                {
                    return;
                }
            }

            anotherItem.ItemInstance.Amount += itemAmount;
            await session.RemoveItemFromInventory(amount: (short)itemAmount, item: item);

            if (sendPackets)
            {
                session.SendInventoryAddPacket(anotherItem);
            }

            return;
        }

        if (item.InventoryType != anotherItem.InventoryType)
        {
            if (sendPackets)
            {
                session.SendInventoryRemovePacket(item);
            }

            item.Slot = destinationSlot;

            if (sendPackets)
            {
                session.SendInventoryAddPacket(item);
            }

            return;
        }

        item.Slot = destinationSlot;
        if (sendPackets)
        {
            session.SendInventoryAddPacket(item);
        }

        anotherItem.Slot = slot;

        if (sendPackets)
        {
            session.SendInventoryAddPacket(anotherItem);
        }
    }
}