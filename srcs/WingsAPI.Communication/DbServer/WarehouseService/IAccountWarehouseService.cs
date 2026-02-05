using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ServiceContract]
    public interface IAccountWarehouseService
    {
        [OperationContract]
        ValueTask<AccountWarehouseGetItemsResponse> GetItems(AccountWarehouseGetItemsRequest request);

        [OperationContract]
        ValueTask<AccountWarehouseGetItemResponse> GetItem(AccountWarehouseGetItemRequest request);

        [OperationContract]
        ValueTask<AccountWarehouseAddItemResponse> AddItem(AccountWarehouseAddItemRequest request);

        [OperationContract]
        ValueTask<AccountWarehouseWithdrawItemResponse> WithdrawItem(AccountWarehouseWithdrawItemRequest request);

        [OperationContract]
        ValueTask<AccountWarehouseMoveItemResponse> MoveItem(AccountWarehouseMoveItemRequest request);
    }
}