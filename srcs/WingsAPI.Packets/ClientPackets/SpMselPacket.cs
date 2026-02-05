using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets
{
    [PacketHeader("sp_msel")]
    public class SpMselPacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte MissionType { get; set; }
    }
}