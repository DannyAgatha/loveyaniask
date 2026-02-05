using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("bpt")]
    public class BptPacket : ClientPacket
    {
        [PacketIndex(0)]
        public long MinutesUntilSeasonEnd { get; set; }
    }
}