using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class InstantRechargeSpHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _languageService;

    public InstantRechargeSpHandler(IGameLanguageService languageService)
    {
       _languageService = languageService;
    }

    public long[] Vnums =>
    [
        (long)ItemVnums.INSTANT_RECHARGE_SPECIALIST_CARD,
    ];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        GameItemInstance itemInstance = e.Item.ItemInstance;

        if (character == null || itemInstance == null)
        {
            return;
        }

        character.SpPointsBasic += e.Item.ItemInstance.GameItem.EffectValue;
        session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);

        await session.RemoveItemFromInventory(item: e.Item);
        session.RefreshStat();
    }
}