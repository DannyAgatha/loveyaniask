// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("pjoin")]
    public class PJoinPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public GroupRequestType RequestType { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        #endregion
    }
}