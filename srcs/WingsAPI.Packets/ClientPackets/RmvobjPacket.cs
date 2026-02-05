// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("rmvobj")]
    public class RmvobjPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Slot { get; set; }

        #endregion
    }
}