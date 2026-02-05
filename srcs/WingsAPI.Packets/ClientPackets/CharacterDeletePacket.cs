// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("Char_DEL")]
    public class CharacterDeletePacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte Slot { get; set; }

        [PacketIndex(1)]
        public string AccountPassword { get; set; }
    }
}