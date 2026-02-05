// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("select")]
    public class SelectPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Slot { get; set; }

        #endregion
    }
}