// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("mg")]
    public class MinigamePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public byte Id { get; set; }

        [PacketIndex(2)]
        public short MinigameVNum { get; set; }

        [PacketIndex(3)]
        public long? Point { get; set; }

        [PacketIndex(4)]
        public long? Point2 { get; set; }

        #endregion
    }
}