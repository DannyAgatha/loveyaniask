using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

/// <summary>
///     This handler is called when you try to use Atk/Def/Hp/Exp,Capsules potions
/// </summary>
public class StatPotionHandler : IItemUsageByVnumHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IBuffFactory _buffFactory;

    public StatPotionHandler(IBCardEffectHandlerContainer bCardEffectHandlerContainer, IBuffFactory buffFactory)
    {
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _buffFactory = buffFactory;
    }

    public long[] Vnums => new[]
    {
        (long)ItemVnums.ATTACK_POTION, (long)ItemVnums.ATTACK_POTION_LIMITED,
        (long)ItemVnums.DEFENCE_POTION, (long)ItemVnums.DEFENCE_POTION_LIMITED,
        (long)ItemVnums.ENERGY_POTION, (long)ItemVnums.ENERGY_POTION_LIMITED,
        (long)ItemVnums.EXPERIENCE_POTION, (long)ItemVnums.EXPERIENCE_POTION_LIMITED,
    };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        await session.RemoveItemFromInventory(item: e.Item);
    }
}