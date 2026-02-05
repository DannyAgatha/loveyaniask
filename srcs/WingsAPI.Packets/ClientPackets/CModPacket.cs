// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("c_mod")]
    public class CmodPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(2)]
        public long BazaarId { get; set; }

        [PacketIndex(3)]
        public short VNum { get; set; }

        [PacketIndex(4)]
        public int Amount { get; set; }

        [PacketIndex(5)]
        public long NewPricePerItem { get; set; }

        [PacketIndex(6)]
        public byte Confirmed { get; set; }

        #endregion
    }
}