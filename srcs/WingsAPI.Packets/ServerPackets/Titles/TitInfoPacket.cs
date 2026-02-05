using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ServerPackets.Titles
{
    [PacketHeader("titinfo")]
    public class TitInfoPacket : ServerPacket, IServerPacket
    {
        [PacketIndex(0)]
        public VisualType VisualType { get; set; }

        [PacketIndex(1)]
        public long VisualId { get; set; }

        [PacketIndex(2)]
        public int VisibleTitleVnum { get; set; }

        [PacketIndex(3)]
        public int EffectTitleVnum { get; set; }
    }
}