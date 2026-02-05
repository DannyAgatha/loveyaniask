// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("mall")]
    public class MallPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int Type { get; set; }

        #endregion
    }
}