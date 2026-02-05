// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("m_shop")]
    public class MShopPacket : ClientPacket
    {
        [PacketIndex(0)]
        public MShopPacketType Type { get; set; }

        //TODO: With the actual deserialization it is impossible to represent this packet's structure
        [PacketIndex(1, SerializeToEnd = true)]
        public string PacketData { get; set; }
    }
}