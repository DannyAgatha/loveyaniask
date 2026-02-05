// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("revival")]
    public class RevivalPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        #endregion
    }
}