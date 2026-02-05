// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("suctl")]
    public class SuctlPacket : ClientPacket
    {
        [PacketIndex(0)]
        public int CastId { get; set; }

        [PacketIndex(1)]
        public int Unknown2 { get; set; }

        [PacketIndex(2)]
        public int MateTransportId { get; set; }

        [PacketIndex(3)]
        public VisualType TargetType { get; set; }

        [PacketIndex(4)]
        public long TargetId { get; set; }

        public override string ToString() => $"{CastId} {Unknown2} {MateTransportId} {TargetType} {TargetId}";
    }
}