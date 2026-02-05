using WingsAPI.Data.Account;

namespace DatabaseServer.Managers
{
    public class AddWarehouseItemResult
    {
        public bool Success { get; init; }

        public AccountWarehouseItemDto UpdatedItem { get; init; }
    }
}