using WingsAPI.Data.Families;
using WingsEmu.DTOs.Items;

namespace FamilyServer.Managers
{
    public class WithdrawWarehouseItemResult
    {
        public bool Success { get; init; }

        public FamilyWarehouseItemDto UpdatedItem { get; init; }

        public ItemInstanceDTO WithdrawnItem { get; init; }
    }
}