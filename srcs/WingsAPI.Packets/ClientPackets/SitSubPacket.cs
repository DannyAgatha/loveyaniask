// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("sit_sub_packet")] // header will be ignored for serializing just sub list packets
    public class SitSubPacket : ClientPacket
    {
        [PacketIndex(0)]
        public VisualType VisualType { get; set; }

        [PacketIndex(1)]
        public long UserId { get; set; }
    }
}