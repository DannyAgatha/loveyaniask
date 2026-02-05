namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("arena")]
    public class ArenaPacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte ArenaType { get; set; }
    }
}