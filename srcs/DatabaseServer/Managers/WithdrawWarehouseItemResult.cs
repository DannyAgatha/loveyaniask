using WingsAPI.Data.Account;
using WingsEmu.DTOs.Items;

namespace DatabaseServer.Managers
{
    public class WithdrawWarehouseItemResult
    {
        public bool Success { get; init; }

        public AccountWarehouseItemDto UpdatedItem { get; init; }

        public ItemInstanceDTO WithdrawnItem { get; init; }
    }
}