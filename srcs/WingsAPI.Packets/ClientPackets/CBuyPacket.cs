// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("c_buy")]
    public class CBuyPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long BazaarItemId { get; set; }

        [PacketIndex(1)]
        public short VNum { get; set; }

        [PacketIndex(2)]
        public int Amount { get; set; }

        [PacketIndex(3)]
        public long Price { get; set; }

        #endregion
    }
}