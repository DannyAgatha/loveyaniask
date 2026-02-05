// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class IceFlowerOilHandler : IItemUsageByVnumHandler
{
    private readonly IBuffFactory _buffFactory;

    public IceFlowerOilHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public long[] Vnums => new[]
    {
        (long)ItemVnums.STRONG_ICE_FLOWER_OIL, (long)ItemVnums.ICE_FLOWER_OIL,
        (long)ItemVnums.LARGE_HEAT_RESISTANCE_POTION, (long)ItemVnums.HEAT_RESISTANCE_POTION
    };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        TimeSpan duration = TimeSpan.Zero;
        switch (e.Item.ItemInstance.ItemVNum)
        {
            case (int)ItemVnums.LARGE_HEAT_RESISTANCE_POTION:
            case (int)ItemVnums.STRONG_ICE_FLOWER_OIL:
                duration = TimeSpan.FromHours(2);
                break;
            case (int)ItemVnums.HEAT_RESISTANCE_POTION:
            case (int)ItemVnums.ICE_FLOWER_OIL:
                duration = TimeSpan.FromMinutes(10);
                break;
        }

        if (session.PlayerEntity.HasBuff(BuffVnums.ICE_FLOWER))
        {
            Buff icebuff = session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.ICE_FLOWER);
            double minutes = (icebuff.Start - DateTime.UtcNow + icebuff.Duration).TotalMinutes;
            var newer = TimeSpan.FromMinutes(duration.TotalMinutes + minutes);
            if (newer.TotalHours >= 2)
            {
                return;
            }
            session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.ICE_FLOWER).SetBuffDuration(newer);
            session.SendStaticBuffUiPacket(session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.ICE_FLOWER), session.PlayerEntity.BuffComponent.GetBuff(340).RemainingTimeInMilliseconds());
        }
        else
        {
            await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.ICE_FLOWER,
             session.PlayerEntity, duration, BuffFlag.BIG_AND_KEEP_ON_LOGOUT, true));
        }

        foreach (IMateEntity pet in session.PlayerEntity.MateComponent.TeamMembers())
        {
            Buff petBuff = pet.BuffComponent.GetBuff((short)BuffVnums.ACT_52_FIRE_DEBUFF);
            if (petBuff != null)
            {
                await pet.RemoveBuffAsync(true, petBuff);
            }
        }

        Buff buff = session.PlayerEntity.BuffComponent.GetBuff((short)BuffVnums.ACT_52_FIRE_DEBUFF);
        await session.PlayerEntity.RemoveBuffAsync(true, buff);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}