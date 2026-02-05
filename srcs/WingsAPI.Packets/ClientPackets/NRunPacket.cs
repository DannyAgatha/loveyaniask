// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("n_run")]
    public class NRunPacket : ClientPacket
    {
        [PacketIndex(0)]
        public NpcRunType Type { get; set; }

        [PacketIndex(1)]
        public int Argument { get; set; }

        [PacketIndex(2)]
        public VisualType Value { get; set; }

        [PacketIndex(3)]
        public int NpcId { get; set; }

        [PacketIndex(4)]
        public byte? Confirmation { get; set; }
    }
}