// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("rxit")]
    public class RxitPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte State { get; set; }

        #endregion
    }
}