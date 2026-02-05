// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("ta_call")]
    public class TaCallPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte CalledIndex { get; set; }

        #endregion
    }
}