// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("ranksk")]
    public class RankSkPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte slot { get; set; }

        #endregion
    }
}