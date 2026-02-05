// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("addobj")]
    public class AddobjPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Slot { get; set; }

        [PacketIndex(1)]
        public short PositionX { get; set; }

        [PacketIndex(2)]
        public short PositionY { get; set; }

        #endregion
    }
}