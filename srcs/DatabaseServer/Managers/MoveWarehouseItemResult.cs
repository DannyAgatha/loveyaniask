using WingsAPI.Data.Account;

namespace DatabaseServer.Managers
{
    public class MoveWarehouseItemResult
    {
        public bool Success { get; init; }

        public AccountWarehouseItemDto OldItem { get; init; }

        public AccountWarehouseItemDto NewItem { get; init; }
    }
}