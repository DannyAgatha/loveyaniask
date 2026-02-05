using System;
using System.Threading.Tasks;
using Serilog;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class InventoryExpansionHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public InventoryExpansionHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType { get; } = ItemType.Special;
    public long[] Effects => new long[] { 604, 605 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.InventoryExpansion))
        {
            return;
        }

        DateTime? effectDateEnd;
        switch (e.Item.ItemInstance.ItemVNum)
        {
            case (int)ItemVnums.INVENTORY_EXPANSION_TICKET_30_DAYS:
                effectDateEnd = DateTime.UtcNow.AddDays(30);
                break;
            case (int)ItemVnums.INVENTORY_EXPANSION_TICKET_60_DAYS:
                effectDateEnd = DateTime.UtcNow.AddDays(60);
                break;
            case (int)ItemVnums.INVENTORY_EXPANSION_TICKET_PERMANENT:
                effectDateEnd = null;
                break;
            default:
                Log.Warning("Well, seems like another item that I didn't expect has the same effect.");
                effectDateEnd = default;
                break;
        }

        await session.EmitEventAsync(new AddStaticBonusEvent(new CharacterStaticBonusDto
        {
            DateEnd = effectDateEnd,
            ItemVnum = e.Item.ItemInstance.GameItem.Id,
            StaticBonusType = StaticBonusType.InventoryExpansion
        }));

        await session.RemoveItemFromInventory(item: e.Item);
        session.ShowInventoryExtensions();

        string name = _gameLanguage.GetLanguage(GameDataType.Item, e.Item.ItemInstance.GameItem.Name, session.UserLanguage);
        session.SendChatMessage(
            _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_EFFECT_ACTIVATED, session.UserLanguage, name),
            ChatMessageColorType.Green);
    }
}