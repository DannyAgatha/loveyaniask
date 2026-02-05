// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("ncif")]
    public class NcifPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public long TargetId { get; set; }

        #endregion
    }
}