using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class AncelloanHeroicBlessingHandler : IItemHandler
{
    private const int CARD_ID = 4662;
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;

    public AncelloanHeroicBlessingHandler(IGameLanguageService gameLanguage, IBuffFactory buffFactory)
    {
        _gameLanguage = gameLanguage;
        _buffFactory = buffFactory;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 30001 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        InventoryItem inv = e.Item;
        Buff buff4662 = session.PlayerEntity.BuffComponent.GetBuff(CARD_ID);
        if (buff4662 != null)
        {
            string buffName = _gameLanguage.GetLanguage(GameDataType.Card, buff4662.Name, session.UserLanguage);
            session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_TWICE, session.UserLanguage, buffName), ChatMessageColorType.Yellow);
            return;
        }

        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateOneHourBuff(session.PlayerEntity, CARD_ID, BuffFlag.BIG_AND_KEEP_ON_LOGOUT));
        await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
    }
}