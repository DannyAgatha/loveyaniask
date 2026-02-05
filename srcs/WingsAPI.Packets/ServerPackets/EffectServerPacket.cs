// NosEmu
// 


namespace WingsEmu.Packets.ServerPackets
{
    [PacketHeader("eff")]
    public class EffectServerPacket : ServerPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte EffectType { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        [PacketIndex(2)]
        public int Id { get; set; }

        #endregion
    }
}