using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets;

[PacketHeader("fws")]
public class FwsPacket : ClientPacket
{
    [PacketIndex(0)]
    public int ItemVnum { get; set; }
}