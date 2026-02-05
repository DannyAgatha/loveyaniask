using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("bp_msel")]
    public class BpMSelPacket : ClientPacket
    {
        [PacketIndex(0)]
        public long QuestId { get; set; }
    }
}