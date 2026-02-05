// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("pdtse")]
    public class PdtsePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public short VNum { get; set; }

        [PacketIndex(2)]
        public int? Unknown { get; set; }

        [PacketIndex(3)]
        public short? EqItemSlot { get; set; }

        [PacketIndex(4)]
        public int? AnotherUnknown { get; set; }

        #endregion
    }
}