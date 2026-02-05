using System;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Equipment.Box;

public class MagicSpeedBoosterHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Box;
    public long[] Effects => [888];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IGameItem item = e.Item.ItemInstance.GameItem;

        if (item.ItemSubType != 7) return;

        InventoryItem invItem = e.Item;
        if (invItem.ItemInstance.IsBound)
        {
            await session.EmitEventAsync(new SpeedBoosterEvent());
            return;
        }

        if (e.Option == 0)
        {
            session.SendPacket("u_i");
            session.SendQnaiPacket(session.GenerateLimitedUseGuriPacket((short)GuriType.UseUntradableItem, item.Id, (long)invItem.Slot), Game18NConstString.RequestToUseObjectThatWillBecameNotExchangeable);
            return;
        }

        invItem.ItemInstance.BoundCharacterId = session.PlayerEntity.Id;

        invItem.ItemInstance.ItemDeleteTime = invItem.ItemInstance.GameItem.ItemValidTime switch
        {
            -1 => null,
            > 0 => DateTime.UtcNow.AddSeconds(invItem.ItemInstance.GameItem.ItemValidTime),
            _ => invItem.ItemInstance.ItemDeleteTime
        };

        session.SendInventoryAddPacket(invItem);
    }
}