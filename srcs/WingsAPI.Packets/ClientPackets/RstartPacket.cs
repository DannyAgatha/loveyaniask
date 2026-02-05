// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("rstart")]
    public class RStartPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte? Type { get; set; }

        #endregion
    }
}