using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class FireworkHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Event;
    public long[] Effects => new long[] { 800 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        int effect = e.Item.ItemInstance.GameItem.Data[2];
        int sound = e.Item.ItemInstance.GameItem.Data[3];
        session.BroadcastEffect(effect);
        session.Broadcast(session.GenerateSound((short)sound), new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}