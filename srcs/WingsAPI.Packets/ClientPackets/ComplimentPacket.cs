// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("compl")]
    public class ComplimentPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(1)]
        public long CharacterId { get; set; }

        #endregion
    }
}