// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("taw")]
    public class TawPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public string Username { get; set; }

        #endregion
    }
}