// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader(":")]
    public class FamilyChatPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0, serializeToEnd: true)]
        public string Message { get; set; }

        #endregion
    }
}