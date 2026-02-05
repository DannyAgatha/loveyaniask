using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class LuiniaHandler : IItemUsageByVnumHandler
{
    public long[] Vnums => new[] { (long)ItemVnums.LUINIA_OF_RESTORATION };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        session.BroadcastEffectInRange(EffectType.AngelDignityRestore);
        await session.RemoveItemFromInventory(item: e.Item, amount: 1);

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        IReadOnlyList<INpcEntity> npcs = session.CurrentMapInstance.GetClosestNpcsInRange(session.PlayerEntity.Position, 10);
        foreach (INpcEntity npc in npcs)
        {
            if (!npc.IsProtected && !npc.IsTimeSpaceMate)
            {
                continue;
            }

            npc.BroadcastEffectInRange(EffectType.ShinyDust);
            if (npc.MaxHp == npc.Hp)
            {
                continue;
            }

            int hpToAdd = (int)(1000 + (npc.MaxHp - npc.Hp) * 0.3);
            await npc.EmitEventAsync(new BattleEntityHealEvent
            {
                Entity = npc,
                HpHeal = hpToAdd
            });

            session.SendStPacket(npc);
        }
    }
}