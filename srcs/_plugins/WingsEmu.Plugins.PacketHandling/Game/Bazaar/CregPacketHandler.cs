using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CregPacketHandler : GenericGamePacketHandlerBase<CRegPacket>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IGameLanguageService _languageService;

    public CregPacketHandler(IGameLanguageService languageService, BazaarConfiguration bazaarConfiguration)
    {
        _languageService = languageService;
        _bazaarConfiguration = bazaarConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CRegPacket cRegPacket)
    {
        DateTime currentDate = DateTime.UtcNow;
        if (session.PlayerEntity.LastListItemBazaar > currentDate)
        {
            return;
        }

        session.PlayerEntity.LastListItemBazaar = currentDate.AddSeconds(_bazaarConfiguration.DelayServerBetweenRequestsInSecs);

        bool hasMedal = session.PlayerEntity.HaveStaticBonus(StaticBonusType.BazaarMedalGold) || session.PlayerEntity.HaveStaticBonus(StaticBonusType.BazaarMedalSilver);

        if (BazaarExtensions.PriceOrAmountExceeds(hasMedal, cRegPacket.PricePerItem, cRegPacket.Amount))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Exceeding the price or amount limit");
            return;
        }

        long totalPrice = cRegPacket.PricePerItem * cRegPacket.Amount;

        short days = cRegPacket.Durability switch
        {
            1 => 1,
            2 when hasMedal => 7,
            3 when hasMedal => 15,
            4 when hasMedal => 30,
            _ => -1
        };

        if (days == -1)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Invalid day amount");
            return;
        }

        long tax = hasMedal ? BazaarExtensions.MedalTax(totalPrice, days) : BazaarExtensions.NormalTax(totalPrice);

        if (tax != cRegPacket.Taxe || !session.HasEnoughGold(tax))
        {
            session.SendInfo(_languageService.GetLanguage(GameDialogKey.BAZAAR_INFO_RESYNC, session.UserLanguage));
            return;
        }

        cRegPacket.Inventory = cRegPacket.Inventory == 4 ? (byte)0 :
            cRegPacket.Inventory > 7 ? (byte)(cRegPacket.Inventory - 8) : cRegPacket.Inventory;

        if (!Enum.IsDefined(typeof(InventoryType), cRegPacket.Inventory))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Received an InventoryType that is not defined in our enum");
            return;
        }

        var inventoryType = (InventoryType)cRegPacket.Inventory;

        if (inventoryType != InventoryType.Equipment && inventoryType != InventoryType.Etc && inventoryType != InventoryType.Main)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[BAZAAR] Trying to put an item whose InventoryType is not allowed.");
            return;
        }

        DateTime expiryDate = DateTime.UtcNow.AddDays(days);

        InventoryItem inventoryItem = session.PlayerEntity.GetItemBySlotAndType(cRegPacket.Slot, inventoryType);

        await session.EmitEventAsync(new BazaarItemAddEvent
        {
            InventoryItem = inventoryItem,
            Amount = cRegPacket.Amount,
            ExpiryDate = expiryDate,
            DayExpiryAmount = days,
            PricePerItem = cRegPacket.PricePerItem,
            UsedMedal = hasMedal,
            IsPackage = cRegPacket.IsPackage == 1,
            Tax = tax
        });
    }
}