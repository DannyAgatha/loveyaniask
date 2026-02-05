// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("blins")]
    public class BlInsPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public long CharacterId { get; set; }

        #endregion
    }
}