// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("f_deposit")]
    public class FDepositPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public InventoryType Inventory { get; set; }

        [PacketIndex(1)]
        public short SourceSlot { get; set; }

        [PacketIndex(2)]
        public int Amount { get; set; }

        [PacketIndex(3)]
        public short DestinationSlot { get; set; }

        [PacketIndex(4)]
        public byte? Unknown { get; set; }

        #endregion
    }
}