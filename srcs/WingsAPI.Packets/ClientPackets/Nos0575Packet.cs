namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("NoS0575")]
    public class Nos0575Packet : ClientPacket
    {
        [PacketIndex(0)]
        public int Number { get; set; }

        [PacketIndex(1)]
        public string Name { get; set; }

        [PacketIndex(2)]
        public string Password { get; set; }

        [PacketIndex(3)]
        public string ClientData { get; set; }

        [PacketIndex(4)]
        public string Hash { get; set; }

        [PacketIndex(5)]
        public string RegionCode { get; set; }

        [PacketIndex(6)]
        public string ClientVersion { get; set; }

        [PacketIndex(7)]
        public string ClientChecksum { get; set; }
    }
}