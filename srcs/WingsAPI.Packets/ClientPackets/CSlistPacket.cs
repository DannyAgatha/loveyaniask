// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("c_slist")]
    public class CsListPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public ushort Index { get; set; }

        [PacketIndex(1)]
        public byte Filter { get; set; }

        #endregion
    }
}