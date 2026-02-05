// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("btk")]
    public class BtkPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long CharacterId { get; set; }
        
        [PacketIndex(1)]
        public string SenderName { get; set; }

        [PacketIndex(2, serializeToEnd: true)]
        public string Message { get; set; }

        #endregion
    }
}