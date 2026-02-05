using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class DyeBombHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;

    private readonly HashSet<ItemVnums> _wigs = new()
    {
        ItemVnums.WIG, ItemVnums.COLORFUL_WIG, ItemVnums.BROWN_WIG, ItemVnums.SPIKY_HAIRSTYLE, ItemVnums.RARE_SPIKY_HAIRSTYLE
    };

    public DyeBombHandler(IGameLanguageService languageService, IRandomGenerator randomGenerator)
    {
        _languageService = languageService;
        _randomGenerator = randomGenerator;
    }

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 30 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        GameItemInstance hat = session.PlayerEntity.Hat;

        if (hat == null)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_WIG, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!_wigs.Contains((ItemVnums)hat.ItemVNum))
        {
            session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        hat.Design = (short)_randomGenerator.RandomNumber(15);
        session.BroadcastEq();
        await session.RemoveItemFromInventory(item: e.Item);
    }
}