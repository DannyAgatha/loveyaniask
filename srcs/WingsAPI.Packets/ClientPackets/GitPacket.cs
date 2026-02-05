// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("git")]
    public class GitPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int ButtonId { get; set; }

        #endregion
    }
}