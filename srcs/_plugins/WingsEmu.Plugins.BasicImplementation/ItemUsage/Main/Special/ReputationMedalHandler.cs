// NosEmu
// 


using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class ReputationMedalHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 69 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        await session.EmitEventAsync(new GenerateReputationEvent
        {
            Amount = (int)e.Item.ItemInstance.GameItem.ReputPrice,
            SendMessage = true
        });

        await session.RemoveItemFromInventory(item: e.Item);
    }
}