using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class MagicalBuffPotionHandler : IItemHandler
{
    private readonly IBuffFactory _buffFactory;

    public MagicalBuffPotionHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 20 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        int buffVnum = e.Item.ItemInstance.GameItem.EffectValue;
        Buff buffToCreate = _buffFactory.CreateBuff(buffVnum, session.PlayerEntity);
        await session.PlayerEntity.AddBuffAsync(buffToCreate);

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            if (!mate.IsAlive())
            {
                continue;
            }

            buffToCreate = _buffFactory.CreateBuff(buffVnum, mate);
            await mate.AddBuffAsync(buffToCreate);
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}