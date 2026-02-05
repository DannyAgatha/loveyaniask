// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("buy")]
    public class BuyPacket : ClientPacket
    {
        [PacketIndex(0)]
        public BuyShopType Type { get; set; }

        [PacketIndex(1)]
        public long OwnerId { get; set; }

        [PacketIndex(2)]
        public short Slot { get; set; }

        [PacketIndex(3)]
        public int Amount { get; set; }
    }
}