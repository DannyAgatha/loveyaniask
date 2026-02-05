// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("deposit")]
    public class DepositPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public InventoryType Inventory { get; set; }

        [PacketIndex(1)]
        public byte Slot { get; set; }

        [PacketIndex(2)]
        public int Amount { get; set; }

        [PacketIndex(3)]
        public byte NewSlot { get; set; }

        [PacketIndex(4)]
        public bool PartnerBackpack { get; set; }

        #endregion
    }
}