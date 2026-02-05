// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("frank_cts")]
    public class FrankCtsPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        #endregion
    }
}