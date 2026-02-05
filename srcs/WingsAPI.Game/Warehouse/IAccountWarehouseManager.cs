using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Account;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Warehouse;

public interface IAccountWarehouseManager
{
    public Task<(IDictionary<short, AccountWarehouseItemDto> accountWarehouseItemDtos, ManagerResponseType?)> GetWarehouse(long accountId);
    public Task<(AccountWarehouseItemDto, ManagerResponseType?)> GetWarehouseItem(long accountId, short slot);
    public Task<(AccountWarehouseItemDto, ManagerResponseType?)> AddWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToAdd);
    public Task<(AccountWarehouseItemDto, ItemInstanceDTO, ManagerResponseType?)> WithdrawWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToWithdraw, int amount);
    public Task<(AccountWarehouseItemDto oldItem, AccountWarehouseItemDto newItem, ManagerResponseType?)> MoveWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot);
    public void CleanCache(long accountId);
}