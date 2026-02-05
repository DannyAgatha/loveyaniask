// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader(EntryPointPacketHeader)]
    public class EntryPointPacket : ClientPacket
    {
        public const string EntryPointPacketHeader = "WingsEmu.EntryPoint";

        [PacketIndex(0)]
        public string SessionName { get; set; }

        [PacketIndex(1)]
        public string Password { get; set; }
    }
}