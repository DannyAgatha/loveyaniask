// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("gjoin")]
    public class JoinFamilyPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        #endregion
    }
}