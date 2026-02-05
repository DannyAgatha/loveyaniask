using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ServiceContract]
    public interface IFamilyWarehouseService
    {
        [OperationContract]
        ValueTask<FamilyWarehouseGetLogsResponse> GetLogs(FamilyWarehouseGetLogsRequest request);

        [OperationContract]
        ValueTask<FamilyWarehouseGetItemsResponse> GetItems(FamilyWarehouseGetItemsRequest request);

        [OperationContract]
        ValueTask<FamilyWarehouseGetItemResponse> GetItem(FamilyWarehouseGetItemRequest request);

        [OperationContract]
        ValueTask<FamilyWarehouseAddItemResponse> AddItem(FamilyWarehouseAddItemRequest request);

        [OperationContract]
        ValueTask<FamilyWarehouseWithdrawItemResponse> WithdrawItem(FamilyWarehouseWithdrawItemRequest request);

        [OperationContract]
        ValueTask<FamilyWarehouseMoveItemResponse> MoveItem(FamilyWarehouseMoveItemRequest request);
    }
}