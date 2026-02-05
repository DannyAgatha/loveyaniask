using WingsAPI.Data.Families;

namespace FamilyServer.Managers
{
    public class AddWarehouseItemResult
    {
        public bool Success { get; init; }

        public FamilyWarehouseItemDto UpdatedItem { get; init; }
    }
}