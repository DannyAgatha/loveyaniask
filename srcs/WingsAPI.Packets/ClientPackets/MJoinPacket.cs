// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("mjoin")]
    public class MJoinPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        [PacketIndex(2)]
        public byte OptionType { get; set; }

        #endregion
    }
}