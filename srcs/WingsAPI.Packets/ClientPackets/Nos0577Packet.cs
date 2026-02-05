using WingsEmu.Packets;

namespace WingsAPI.Packets.ClientPackets;

[PacketHeader("NoS0577")]
public class Nos0577Packet : ClientPacket
{
    [PacketIndex(0)]
    public string SessionHex { get; set; }
        
    [PacketIndex(1)]
    public string Hwid { get; set; }
        
    [PacketIndex(2)]
    public string RandomHex { get; set; }
        
    [PacketIndex(3)]
    public string ClientVersion { get; set; }
        
    [PacketIndex(4)]
    public string ClientLanguage { get; set; }
        
    [PacketIndex(5)]
    public string Md5 { get; set; }
}