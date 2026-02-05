using WingsAPI.Data.Families;

namespace FamilyServer.Managers
{
    public class MoveWarehouseItemResult
    {
        public bool Success { get; init; }

        public FamilyWarehouseItemDto OldItem { get; init; }

        public FamilyWarehouseItemDto NewItem { get; init; }
    }
}