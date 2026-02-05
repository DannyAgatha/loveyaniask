// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("qt")]
    public class QtPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Action { get; set; }

        [PacketIndex(1)]
        public int Slot { get; set; }

        #endregion
    }
}