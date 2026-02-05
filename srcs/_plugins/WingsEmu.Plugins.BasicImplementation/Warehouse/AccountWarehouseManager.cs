using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.WarehouseService;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.Warehouse;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public class AccountWarehouseManager : IAccountWarehouseManager
{
    private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(15);

    private readonly IAccountWarehouseService _accountWarehouseService;

    private readonly ILongKeyCachedRepository<Dictionary<short, AccountWarehouseItemDto>> _cachedAccountItems;

    public AccountWarehouseManager(ILongKeyCachedRepository<Dictionary<short, AccountWarehouseItemDto>> cachedAccountItems, IAccountWarehouseService accountWarehouseService)
    {
        _cachedAccountItems = cachedAccountItems;
        _accountWarehouseService = accountWarehouseService;
    }

    public async Task<(IDictionary<short, AccountWarehouseItemDto> accountWarehouseItemDtos, ManagerResponseType?)> GetWarehouse(long accountId)
    {
        Dictionary<short, AccountWarehouseItemDto> retrievedItems = _cachedAccountItems.Get(accountId);

        if (retrievedItems != null)
        {
            _cachedAccountItems.Set(accountId, retrievedItems, LifeTime);
            return (retrievedItems, ManagerResponseType.Success);
        }

        AccountWarehouseGetItemsResponse response = null;

        try
        {
            response = await _accountWarehouseService.GetItems(new AccountWarehouseGetItemsRequest
            {
                AccountId = accountId
            });
        }
        catch (Exception ex)
        {
            Log.Error("[ACCOUNT_WAREHOUSE_MANAGER][GET_ITEMS] ", ex);
        }

        Dictionary<short, AccountWarehouseItemDto> dictionary = response?.Items?.ToDictionary(x => x.Slot) ?? new Dictionary<short, AccountWarehouseItemDto>();

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            _cachedAccountItems.Set(accountId, dictionary, LifeTime);
        }

        return (dictionary, response?.ResponseType.ToManagerType());
    }

    public async Task<(AccountWarehouseItemDto, ManagerResponseType?)> GetWarehouseItem(long accountId, short slot)
    {
        (IDictionary<short, AccountWarehouseItemDto> familyWarehouseItemDtos, ManagerResponseType? responseType) = await GetWarehouse(accountId);
        return (familyWarehouseItemDtos.GetOrDefault(slot), responseType);
    }

    public async Task<(AccountWarehouseItemDto, ManagerResponseType?)> AddWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToAdd)
    {
        AccountWarehouseAddItemResponse response = null;

        try
        {
            response = await _accountWarehouseService.AddItem(new AccountWarehouseAddItemRequest
            {
                Item = warehouseItemDtoToAdd
            });
        }
        catch (Exception ex)
        {
            Log.Error("[ACCOUNT_WAREHOUSE_MANAGER][ADD_ITEM] ", ex);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            await UpdateWarehouseItem(warehouseItemDtoToAdd.AccountId, response.Item.Slot, response.Item);
        }

        return (response?.Item, response?.ResponseType.ToManagerType());
    }

    public async Task<(AccountWarehouseItemDto, ItemInstanceDTO, ManagerResponseType?)> WithdrawWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToWithdraw, int amount)
    {
        AccountWarehouseWithdrawItemResponse response = null;

        try
        {
            response = await _accountWarehouseService.WithdrawItem(new AccountWarehouseWithdrawItemRequest
            {
                ItemToWithdraw = warehouseItemDtoToWithdraw,
                Amount = amount
            });
        }
        catch (Exception ex)
        {
            Log.Error("[ACCOUNT_WAREHOUSE_MANAGER][WITHDRAW_ITEM] ", ex);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            await UpdateWarehouseItem(warehouseItemDtoToWithdraw.AccountId, warehouseItemDtoToWithdraw.Slot, response.UpdatedItem);
        }

        return (response?.UpdatedItem, response?.WithdrawnItem, response?.ResponseType.ToManagerType());
    }

    public async Task<(AccountWarehouseItemDto oldItem, AccountWarehouseItemDto newItem, ManagerResponseType?)> MoveWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToMove, int amount,
        short newSlot)
    {
        AccountWarehouseMoveItemResponse response = null;

        try
        {
            response = await _accountWarehouseService.MoveItem(new AccountWarehouseMoveItemRequest
            {
                WarehouseItemDtoToMove = warehouseItemDtoToMove,
                Amount = amount,
                NewSlot = newSlot
            });
        }
        catch (Exception ex)
        {
            Log.Error("[ACCOUNT_WAREHOUSE_MANAGER][MOVE_ITEM] ", ex);
        }

        if (response?.ResponseType == RpcResponseType.SUCCESS)
        {
            await UpdateWarehouseItem(warehouseItemDtoToMove.AccountId, warehouseItemDtoToMove.Slot, response.OldItem);
            await UpdateWarehouseItem(warehouseItemDtoToMove.AccountId, newSlot, response.NewItem);
        }

        return (response?.OldItem, response?.NewItem, response?.ResponseType.ToManagerType());
    }

    public void CleanCache(long accountId)
    {
        _cachedAccountItems.Remove(accountId);
    }

    private async Task UpdateWarehouseItem(long accountId, short slot, AccountWarehouseItemDto dto)
    {
        (IDictionary<short, AccountWarehouseItemDto> items, ManagerResponseType? responseType) = await GetWarehouse(accountId);
        if (responseType != ManagerResponseType.Success)
        {
            return;
        }

        if (dto == null)
        {
            items.Remove(slot);
            return;
        }

        items[slot] = dto;
    }
}