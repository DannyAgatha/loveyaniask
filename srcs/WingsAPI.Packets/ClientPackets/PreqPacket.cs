// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("preq")]
    public class PreqPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public bool Confirmed { get; set; }

        #endregion
    }
}