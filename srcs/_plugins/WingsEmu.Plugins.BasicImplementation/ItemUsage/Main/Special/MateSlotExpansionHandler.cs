using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class MateSlotExpansionHandler : IItemUsageByVnumHandler
{
    /**
         * These values can be configurable if needed
         */
    private const int MaxPet = 90;

    private const int MaxPartner = 12;
    private const int PetExpansionAdd = 10;
    private const int PartnerExpansionAdd = 1;

    private readonly IGameLanguageService _languageService;

    public MateSlotExpansionHandler(IGameLanguageService languageService) => _languageService = languageService;

    public long[] Vnums => new[]
    {
        (long)ItemVnums.PARTNER_SLOT_EXPANSION, (long)ItemVnums.PARTNER_SLOT_EXPANSION_LIMITED,
        (long)ItemVnums.PET_SLOT_EXPANSION, (long)ItemVnums.PET_SLOT_EXPANSION_LIMITED
    };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        bool isPetExpansion = e.Item.ItemInstance.ItemVNum == (int)ItemVnums.PET_SLOT_EXPANSION || e.Item.ItemInstance.ItemVNum == (int)ItemVnums.PET_SLOT_EXPANSION_LIMITED;
        if (isPetExpansion && session.PlayerEntity.MaxPetCount >= MaxPet)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PET_SLOTS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!isPetExpansion && session.PlayerEntity.MaxPartnerCount >= MaxPartner)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PARTNER_SLOTS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (e.Option == 0)
        {
            session.SendPacket(
                $"qna #u_i^1^{session.PlayerEntity.Id}^{(byte)e.Item.ItemInstance.GameItem.Type}^{e.Item.Slot}^2 {_languageService.GetLanguage(isPetExpansion ? GameDialogKey.PET_DIALOG_ASK_SLOT_INCREASE : GameDialogKey.PARTNER_DIALOG_ASK_SLOT_INCREASE, session.UserLanguage)}");
            return;
        }

        if (isPetExpansion)
        {
            session.PlayerEntity.MaxPetCount += PetExpansionAdd;
        }
        else
        {
            session.PlayerEntity.MaxPartnerCount += PartnerExpansionAdd;
        }

        string itemName = e.Item.ItemInstance.GameItem.GetItemName(_languageService, session.UserLanguage);

        session.SendChatMessage(_languageService.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_EFFECT_ACTIVATED, session.UserLanguage, itemName), ChatMessageColorType.Green);
        session.SendScpStcPacket();

        await session.RemoveItemFromInventory(item: e.Item);
    }
}