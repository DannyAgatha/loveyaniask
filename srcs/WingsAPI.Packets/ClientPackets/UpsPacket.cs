// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("u_ps")]
    public class UpsPacket : ClientPacket
    {
        [PacketIndex(0)]
        public int MateTransportId { get; set; }

        [PacketIndex(1)]
        public VisualType TargetType { get; set; }

        [PacketIndex(2)]
        public int TargetId { get; set; }

        [PacketIndex(3)]
        public int SkillSlot { get; set; }

        [PacketIndex(4)]
        public short MapX { get; set; }

        [PacketIndex(5)]
        public short MapY { get; set; }
    }
}