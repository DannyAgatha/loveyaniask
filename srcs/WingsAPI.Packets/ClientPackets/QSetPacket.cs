// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("qset")]
    public class QSetPacket : ClientPacket
    {
        [PacketIndex(0)]
        public QsetPacketType Type { get; set; }

        [PacketIndex(1)]
        public short QuicklistTab { get; set; }

        [PacketIndex(2)]
        public short QuicklistSlot { get; set; }

        [PacketIndex(3)]
        public short? DestinationType { get; set; }

        [PacketIndex(4)]
        public short? DestinationSlotOrVnum { get; set; }
    }
}