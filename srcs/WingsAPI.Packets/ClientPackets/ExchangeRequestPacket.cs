// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("req_exc")]
    public class ExchangeRequestPacket : ClientPacket
    {
        [PacketIndex(0)]
        public RequestExchangeType RequestType { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }
    }
}