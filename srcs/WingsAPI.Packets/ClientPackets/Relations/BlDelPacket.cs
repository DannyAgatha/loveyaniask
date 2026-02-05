// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("bldel")]
    public class BlDelPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long CharacterId { get; set; }

        #endregion
    }
}