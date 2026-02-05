// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("$fl")]
    public class FlPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public string CharName { get; set; }

        #endregion
    }
}