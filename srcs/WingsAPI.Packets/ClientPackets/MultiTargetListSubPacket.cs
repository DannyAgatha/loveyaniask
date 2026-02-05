// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("multi_target_list_sub_packet")] // header will be ignored for serializing just sub list packets
    public class MultiTargetListSubPacket : ClientPacket
    {
        [PacketIndex(0)]
        public VisualType TargetType { get; set; }

        [PacketIndex(1)]
        public int TargetId { get; set; }
    }
}