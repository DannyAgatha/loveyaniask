// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("bsc")]
    public class BscPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public byte? Option { get; set; }

        #endregion
    }
}