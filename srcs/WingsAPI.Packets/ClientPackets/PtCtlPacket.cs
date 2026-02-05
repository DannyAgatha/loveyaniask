// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("ptctl")]
    public class PtCtlPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short MapId { get; set; }

        [PacketIndex(1)]
        public byte Amount { get; set; }

        [PacketIndex(2, SerializeToEnd = true)]
        public string PacketEnd { get; set; }

        #endregion
    }
}