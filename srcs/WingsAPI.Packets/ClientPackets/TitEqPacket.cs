using WingsEmu.Packets.Enums.Titles;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("tit_eq")]
    public class TitEqPacket : ClientPacket
    {
        [PacketIndex(0)]
        public TitEqPacketType Type { get; set; }

        [PacketIndex(1)]
        public int ItemVnum { get; set; }
    }
}