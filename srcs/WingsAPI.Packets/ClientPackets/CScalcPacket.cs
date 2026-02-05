// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("c_scalc")]
    public class CScalcPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long BazaarId { get; set; }

        [PacketIndex(1)]
        public short VNum { get; set; }

        [PacketIndex(2)]
        public int Amount { get; set; }

        [PacketIndex(3)]
        public short MaxAmount { get; set; }

        [PacketIndex(4)]
        public long Price { get; set; }

        #endregion
    }
}