// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("csp")]
    public class CspServerPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long CharacterId { get; set; }

        [PacketIndex(1)]
        public string Message { get; set; }

        #endregion
    }
}