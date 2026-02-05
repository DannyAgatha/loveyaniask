// NosEmu
// 


using System.Collections.Generic;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("mtlist")]
    public class MultiTargetListPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte TargetsAmount { get; set; }

        [PacketIndex(1, RemoveSeparator = true)]
        public List<MultiTargetListSubPacket> Targets { get; set; }

        #endregion
    }
}