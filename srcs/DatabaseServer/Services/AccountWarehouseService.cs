using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseServer.Managers;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.WarehouseService;
using WingsAPI.Data.Account;

namespace DatabaseServer.Services
{
    public class AccountWarehouseService : IAccountWarehouseService
    {
        private readonly IAccountWarehouseManager _accountWarehouseManager;

        public AccountWarehouseService(IAccountWarehouseManager accountWarehouseManager) => _accountWarehouseManager = accountWarehouseManager;

        public async ValueTask<AccountWarehouseGetItemsResponse> GetItems(AccountWarehouseGetItemsRequest request)
        {
            IEnumerable<AccountWarehouseItemDto> warehouseItemDtos = await _accountWarehouseManager.GetWarehouse(request.AccountId);

            return new AccountWarehouseGetItemsResponse
            {
                ResponseType = warehouseItemDtos == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Items = warehouseItemDtos
            };
        }

        public async ValueTask<AccountWarehouseGetItemResponse> GetItem(AccountWarehouseGetItemRequest request)
        {
            AccountWarehouseItemDto warehouseItemDto = await _accountWarehouseManager.GetWarehouseItem(request.AccountId, request.Slot);

            return new AccountWarehouseGetItemResponse
            {
                ResponseType = warehouseItemDto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Item = warehouseItemDto
            };
        }

        public async ValueTask<AccountWarehouseAddItemResponse> AddItem(AccountWarehouseAddItemRequest request)
        {
            AddWarehouseItemResult result = await _accountWarehouseManager.AddWarehouseItem(request.Item);

            return new AccountWarehouseAddItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                Item = result.UpdatedItem
            };
        }

        public async ValueTask<AccountWarehouseWithdrawItemResponse> WithdrawItem(AccountWarehouseWithdrawItemRequest request)
        {
            WithdrawWarehouseItemResult result = await _accountWarehouseManager.WithdrawWarehouseItem(request.ItemToWithdraw, request.Amount);

            return new AccountWarehouseWithdrawItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                UpdatedItem = result.UpdatedItem,
                WithdrawnItem = result.WithdrawnItem
            };
        }

        public async ValueTask<AccountWarehouseMoveItemResponse> MoveItem(AccountWarehouseMoveItemRequest request)
        {
            MoveWarehouseItemResult result = await _accountWarehouseManager.MoveWarehouseItem(request.WarehouseItemDtoToMove, request.Amount, request.NewSlot);

            return new AccountWarehouseMoveItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                OldItem = result.OldItem,
                NewItem = result.NewItem
            };
        }
    }
}