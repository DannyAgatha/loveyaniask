using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class XpFamilyHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public XpFamilyHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public long[] Vnums =>
    [
        (long)ItemVnums.XPF_1000, (long)ItemVnums.XPF_1500, (long)ItemVnums.XPF_2000,
        (long)ItemVnums.XPF_2500, (long)ItemVnums.XPF_3000, (long)ItemVnums.XPF_4500,
        (long)ItemVnums.XPF_5000, (long)ItemVnums.XPF_6000, (long)ItemVnums.XPF_8000,
        (long)ItemVnums.XPF_10000, (long)ItemVnums.XPF_14000, (long)ItemVnums.XPF_16000,
        (long)ItemVnums.XPF_20000, (long)ItemVnums.XPF_24000, (long)ItemVnums.XPF_25000,
        (long)ItemVnums.XPF_30000, (long)ItemVnums.XPF_50000, (long)ItemVnums.XPF_100000,
        (long)ItemVnums.XPF_150000, (long)ItemVnums.XPF_200000, (long)ItemVnums.XPF_300000,
        (long)ItemVnums.XPF_500000, (long)ItemVnums.XPF_750000, (long)ItemVnums.XPF_850000,
        (long)ItemVnums.XPF_1000000, (long)ItemVnums.XPF_1250000, (long)ItemVnums.XPF_1500000
    ];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.Family == null)
        {
            return;
        }
        
        int experienceGained = e.Item.ItemInstance.GameItem.Id switch
        {
            (int)ItemVnums.XPF_1000 => 1000,
            (int)ItemVnums.XPF_1500 => 1500,
            (int)ItemVnums.XPF_2000 => 2000,
            (int)ItemVnums.XPF_2500 => 2500,
            (int)ItemVnums.XPF_3000 => 3000,
            (int)ItemVnums.XPF_4500 => 4500,
            (int)ItemVnums.XPF_5000 => 5000,
            (int)ItemVnums.XPF_6000 => 6000,
            (int)ItemVnums.XPF_8000 => 8000,
            (int)ItemVnums.XPF_10000 => 10000,
            (int)ItemVnums.XPF_14000 => 14000,
            (int)ItemVnums.XPF_16000 => 16000,
            (int)ItemVnums.XPF_20000 => 20000,
            (int)ItemVnums.XPF_24000 => 24000,
            (int)ItemVnums.XPF_25000 => 25000,
            (int)ItemVnums.XPF_30000 => 30000,
            (int)ItemVnums.XPF_50000 => 50000,
            (int)ItemVnums.XPF_100000 => 100000,
            (int)ItemVnums.XPF_150000 => 150000,
            (int)ItemVnums.XPF_200000 => 200000,
            (int)ItemVnums.XPF_300000 => 300000,
            (int)ItemVnums.XPF_500000 => 500000,
            (int)ItemVnums.XPF_750000 => 750000,
            (int)ItemVnums.XPF_850000 => 850000,
            (int)ItemVnums.XPF_1000000 => 1000000,
            (int)ItemVnums.XPF_1250000 => 1250000,
            (int)ItemVnums.XPF_1500000 => 1500000,
            _ => 0
        };
        
        if (experienceGained > 0)
        {
            await session.EmitEventAsync(new FamilyAddExperienceEvent(experienceGained, FamXpObtainedFromType.Item));
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}
