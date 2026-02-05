// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("npinfo")]
    public class NpinfoPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Page { get; set; }

        #endregion
    }
}