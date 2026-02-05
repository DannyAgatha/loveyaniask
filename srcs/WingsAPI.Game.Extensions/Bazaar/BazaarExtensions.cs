using System;
using WingsAPI.Data.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.Bazaar
{
    public static class BazaarExtensions
    {
        public static BazaarListedItemType GetBazaarItemStatus(this BazaarItemDTO item)
        {
            if (item.ExpiryDate > DateTime.UtcNow)
            {
                return (item.Amount - item.SoldAmount) == 0 ? BazaarListedItemType.Sold : BazaarListedItemType.ForSale;
            }

            return (item.Amount - item.SoldAmount) != 0 ? BazaarListedItemType.DeadlineExpired : BazaarListedItemType.Sold;
        }

        public static bool IsBazaarActionBlocked(this IClientSession session) =>
            session.IsActionForbidden() || session.PlayerEntity.IsInExchange() || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsShopping || !session.PlayerEntity.HasNosBazaarOpen
            || session.PlayerEntity.IsWarehouseOpen || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen;

        public static bool PriceOrAmountExceeds(bool hasMedal, long pricePerItem, int amount)
            => pricePerItem < 1 ||  amount is > 9999 or < 1 || (!hasMedal ? pricePerItem > 2_000_000 || pricePerItem * amount > 200_000_000 : pricePerItem * amount > 2_000_000_000);

        public static long NormalTax(long price)
        {
            return price switch
            {
                <= 100_000 => 500,
                >= 20_000_000 => 100_000,
                _ => (long)Math.Floor(price * 0.005)
            };
        }

        public static long MedalTax(long price, short days)
        {
            double multiplier = days * 0.0005;
            long rawResult = (long)((price - price % 2000) * multiplier);

            return rawResult switch
            {
                < 50 => 50,
                > 20_000 => 20_000,
                _ => rawResult
            };
        }
    }
}