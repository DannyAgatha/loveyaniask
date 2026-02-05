// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("say")]
    public class SayPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string Message { get; set; }

        #endregion
    }
}