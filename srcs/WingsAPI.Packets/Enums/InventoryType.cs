// NosEmu
// 


namespace WingsEmu.Packets.Enums
{
    public enum InventoryType : byte
    {
        Equipment = 0,
        Main = 1,
        Etc = 2,
        Miniland = 3,
        Warehouse = 4,
        Partner = 5,
        Specialist = 6,
        Costume = 7,


        EquippedItems = 200 // not present in the game, for DB purpose
    }
}