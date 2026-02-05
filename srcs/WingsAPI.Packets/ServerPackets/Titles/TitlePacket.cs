// NosEmu
// 


using System.Collections.Generic;

namespace WingsEmu.Packets.ServerPackets.Titles
{
    [PacketHeader("title")]
    public class TitlePacket : ServerPacket
    {
        [PacketIndex(0)]
        public List<TitleSubPacket> Titles { get; set; }
    }
}