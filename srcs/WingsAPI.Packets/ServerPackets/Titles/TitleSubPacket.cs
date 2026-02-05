using WingsEmu.Packets.Enums.Titles;

namespace WingsEmu.Packets.ServerPackets.Titles
{
    [PacketHeader("title_subPacket")]
    public class TitleSubPacket : ServerPacket
    {
        [PacketIndex(0)]
        public int ItemVnum { get; set; }

        [PacketIndex(1)]
        public TitleStatus State { get; set; }
    }
}