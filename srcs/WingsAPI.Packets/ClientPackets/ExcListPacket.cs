// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("exc_list")]
    public class ExcListPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0, SerializeToEnd = true)]
        public string PacketData { get; set; }

        #endregion
    }
}