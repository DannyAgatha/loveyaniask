// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("u_pet")]
    public class UpetPacket : ClientPacket
    {
        [PacketIndex(0)]
        public int MateTransportId { get; set; }

        [PacketIndex(1)]
        public VisualType TargetType { get; set; }

        [PacketIndex(2)]
        public int TargetId { get; set; }

        [PacketIndex(3)]
        public int SkillVnum { get; set; }

        public override string ToString() => $"{MateTransportId} {TargetType} {TargetId} 0";
    }
}