// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("b_i")]
    public class BiPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public InventoryType InventoryType { get; set; }

        [PacketIndex(1)]
        public byte Slot { get; set; }

        [PacketIndex(2)]
        public byte? Option { get; set; }

        #endregion
    }
}