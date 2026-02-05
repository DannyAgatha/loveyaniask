// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("u_s")]
    public class UseSkillPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int CastId { get; set; }

        [PacketIndex(1)]
        public VisualType VisualType { get; set; }

        [PacketIndex(2)]
        public int MapMonsterId { get; set; }

        [PacketIndex(3)]
        public short? MapX { get; set; }

        [PacketIndex(4)]
        public short? MapY { get; set; }

        public override string ToString() => $"{CastId} {VisualType} {MapMonsterId} {MapX} {MapY}";

        #endregion
    }
}