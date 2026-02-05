// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("npc_req")]
    public class RequestNpcPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public VisualType VisualType { get; set; }

        [PacketIndex(1)]
        public long TargetNpcId { get; set; }

        #endregion
    }
}