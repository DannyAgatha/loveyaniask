// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class RaidSealHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 301 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        session.PlayerEntity.LastSeal = e.Item;
        await session.EmitEventAsync(new RaidPartyCreateEvent((byte)e.Item.ItemInstance.GameItem.EffectValue, e.Item, e.Item.ItemInstance.GameItem.Id >= 1127));
    }
}