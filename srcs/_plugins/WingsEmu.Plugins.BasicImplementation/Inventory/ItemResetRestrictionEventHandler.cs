using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Items.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class ItemResetRestrictionEventHandler : IAsyncEventProcessor<ItemResetRestrictionEvent>
{
    private readonly IExpirableLockService _lockService;
    private readonly ItemBuyDailyLimitConfiguration _itemBuyDailyLimitConfiguration;

    public ItemResetRestrictionEventHandler(IExpirableLockService lockService, ItemBuyDailyLimitConfiguration itemBuyDailyLimitConfiguration)
    {
        _lockService = lockService;
        _itemBuyDailyLimitConfiguration = itemBuyDailyLimitConfiguration;
    }

    public async Task HandleAsync(ItemResetRestrictionEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!await _lockService.TryAddTemporaryLockAsync($"game:locks:item-restriction:{session.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1)))
        {
            return;
        }
        
        session.PlayerEntity.ItemRestrictionDto.Items = new List<ItemRestrictionDTO>();
    
        foreach (ItemBuyDailyLimit itemBuyDailyLimit in _itemBuyDailyLimitConfiguration.Items)
        {
            var newItemRestrictionDto = new ItemRestrictionDTO
            {
                ItemVnum = itemBuyDailyLimit.ItemVnum,
                Amount = itemBuyDailyLimit.Amount
            };
        
            session.PlayerEntity.ItemRestrictionDto.Items.Add(newItemRestrictionDto);
        }
        
        foreach (ItemBuyDailyLimit itemConfig in _itemBuyDailyLimitConfiguration.Items)
        {
            int itemVNum = itemConfig.ItemVnum;

            ItemRestrictionDTO itemRestrictionDto = session.PlayerEntity.ItemRestrictionDto.Items.SingleOrDefault(item => item.ItemVnum == itemVNum);

            if (itemRestrictionDto != null)
            {
                itemRestrictionDto.Amount = itemConfig.Amount;
            }
        }
    }
}