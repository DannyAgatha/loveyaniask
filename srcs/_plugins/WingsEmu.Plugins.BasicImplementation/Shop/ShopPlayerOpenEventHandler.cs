using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Shop;

public class ShopPlayerOpenEventHandler : IAsyncEventProcessor<ShopPlayerOpenEvent>
{
    private readonly IGameLanguageService _languageService;

    public ShopPlayerOpenEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(ShopPlayerOpenEvent e, CancellationToken cancellation)
    {
        return;
        const int amountPersonalShopItems = 20;
        if (amountPersonalShopItems != e.Items.Count)
        {
            return;
        }

        if (!e.Sender.CurrentMapInstance.ShopAllowed)
        {
            e.Sender.SendShopEndPacket(ShopEndType.Player);
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.SHOP_INFO_NOT_ALLOWED, e.Sender.UserLanguage));
            return;
        }

        if (e.Sender.PlayerEntity.HasShopOpened || e.Sender.PlayerEntity.ShopComponent.Items != null)
        {
            e.Sender.SendShopEndPacket(ShopEndType.Player);
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.SHOP_INFO_ALREADY_OPEN, e.Sender.UserLanguage));
            return;
        }

        if (e.Sender.PlayerEntity.IsInRaidParty)
        {
            e.Sender.SendShopEndPacket(ShopEndType.Player);
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.SHOP_INFO_NOT_ALLOWED_IN_RAID, e.Sender.UserLanguage));
            return;
        }

        if (e.Sender.CurrentMapInstance.Portals.Any(por => Math.Abs(e.Sender.PlayerEntity.PositionX - por.PositionX) < 6 && Math.Abs(e.Sender.PlayerEntity.PositionY - por.PositionY) < 6))
        {
            e.Sender.SendShopEndPacket(ShopEndType.Player);
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.SHOP_INFO_NEAR_PORTAL, e.Sender.UserLanguage));
            return;
        }

        e.Sender.PlayerEntity.ShopComponent.AddShop(e.Items);
        e.Sender.PlayerEntity.ShopComponent.Name = e.ShopTitle;
        e.Sender.PlayerEntity.HasShopOpened = true;
        e.Sender.PlayerEntity.IsShopping = true;

        e.Sender.BroadcastShop();
        e.Sender.BroadcastPlayerShopFlag((long)DialogVnums.SHOP_PLAYER);
        e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.SHOP_INFO_OPEN, e.Sender.UserLanguage));
        e.Sender.SendCondPacket();
        e.Sender.EmitEvent(new PlayerRestEvent
        {
            RestTeamMemberMates = false
        });
        await e.Sender.EmitEventAsync(new ShopOpenedEvent
        {
            ShopName = e.ShopTitle
        });
    }
}