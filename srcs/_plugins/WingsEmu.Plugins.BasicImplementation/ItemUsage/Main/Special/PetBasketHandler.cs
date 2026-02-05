using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class PetBasketHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;
    public PetBasketHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => [252, 603, 1007];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.PetBasket))
        {
            return;
        }

        await session.EmitEventAsync(new AddStaticBonusEvent(new CharacterStaticBonusDto
        {
            DateEnd = EffectDateEndCalculation(e),
            ItemVnum = e.Item.ItemInstance.GameItem.Id,
            StaticBonusType = StaticBonusType.PetBasket
        }));

        await session.RemoveItemFromInventory(item: e.Item);
        session.ShowInventoryExtensions();
        session.SendPetBasketPacket(true);

        string name = _gameLanguage.GetLanguage(GameDataType.Item, e.Item.ItemInstance.GameItem.Name, session.UserLanguage);
        session.SendChatMessage(
            _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_EFFECT_ACTIVATED, session.UserLanguage, name),
            ChatMessageColorType.Green);
    }

    private static DateTime? EffectDateEndCalculation(InventoryUseItemEvent e)
    {
        if (e.Item.ItemInstance.GameItem.Effect == 252)
        {
            return DateTime.UtcNow.AddDays(10);
        }

        return e.Item.ItemInstance.GameItem.EffectValue == 0 ? null : DateTime.UtcNow.AddDays(e.Item.ItemInstance.GameItem.EffectValue);
    }
}