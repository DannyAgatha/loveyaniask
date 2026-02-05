// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("dir")]
    public class DirectionPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Direction { get; set; }

        [PacketIndex(1)]
        public VisualType VisualType { get; set; }

        [PacketIndex(2)]
        public long Id { get; set; }

        #endregion
    }
}