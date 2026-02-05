// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("csp_r")]
    public class CsprPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string Message { get; set; }

        #endregion
    }
}