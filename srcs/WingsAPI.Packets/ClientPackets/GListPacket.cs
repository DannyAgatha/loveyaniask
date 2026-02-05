// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("glist")]
    public class GListPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(1)]
        public GListPacketType Type { get; set; }

        #endregion
    }
}