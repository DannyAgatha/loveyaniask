// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("fins")]
    public class FInsPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        #endregion
    }
}