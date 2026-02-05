// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader(";")]
    public class GroupSayPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string Message { get; set; }

        #endregion
    }
}