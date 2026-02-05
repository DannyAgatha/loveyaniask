// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("put")]
    public class PutPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public InventoryType InventoryType { get; set; }

        [PacketIndex(1)]
        public byte Slot { get; set; }

        [PacketIndex(2)]
        public int Amount { get; set; }

        #endregion
    }
}