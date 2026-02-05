using WingsAPI.Packets.Enums;
using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("bp_psel")]
    public class BpPSelPacket : ClientPacket
    {
        [PacketIndex(0)]
        public BattlePassItemType Type { get; set; }

        [PacketIndex(1)]
        public long BearingId { get; set; }
    }
}
