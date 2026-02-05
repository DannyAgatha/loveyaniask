// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("psl")]
    public class PslPacket : ClientPacket
    {
        [PacketIndex(0)]
        public int Type { get; set; }
    }
}