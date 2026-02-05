using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class ItemSpawnHandler : IItemHandler
{
    private readonly INpcEntityFactory _npcEntityFactory;

    public ItemSpawnHandler(INpcEntityFactory npcEntityFactory) => _npcEntityFactory = npcEntityFactory;

    public ItemType ItemType => ItemType.Special;
    public long[] Effects { get; } = { (short)ItemEffectVnums.SPAWN_NPC };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        INpcEntity npcEntity = _npcEntityFactory.CreateNpc(e.Item.ItemInstance.GameItem.EffectValue, session.CurrentMapInstance, null, new NpcAdditionalData
        {
            NpcShouldRespawn = false
        });
        
        if (npcEntity == null)
        {
            return;
        }

        await session.RemoveItemFromInventory(item: e.Item, amount: 1);
        await npcEntity.EmitEventAsync(new MapJoinNpcEntityEvent(npcEntity, session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
    }
}