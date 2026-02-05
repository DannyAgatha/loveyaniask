// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("gbox")]
    public class GboxPacket : ClientPacket
    {
        [PacketIndex(0)]
        public BankActionType Type { get; set; }

        [PacketIndex(1)]
        public long Amount { get; set; }

        [PacketIndex(2)]
        public byte Option { get; set; }
    }
}