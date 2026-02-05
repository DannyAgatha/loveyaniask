// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("pulse")]
    public class PulsePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int Tick { get; set; }

        [PacketIndex(1)]
        public byte Checksum { get; set; }

        #endregion
    }
}