using WingsAPI.Packets.Enums;
using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets;

[PacketHeader("tchange")]
public class TchangePacket : ClientPacket
{
    [PacketIndex(0)]
    public TattooChangeType Type { get; set; }
    
    [PacketIndex(1)]
    public short TattooVnum { get; set; }
    
    [PacketIndex(2)]
    public short Data { get; set; }
}