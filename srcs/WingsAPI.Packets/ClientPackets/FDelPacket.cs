// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("fdel")]
    public class FDelPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long CharacterId { get; set; }

        #endregion
    }
}