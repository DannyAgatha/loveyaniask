namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("wreq")]
    public class WreqPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Value { get; set; }

        [PacketIndex(1)]
        public long? Param { get; set; }

        #endregion
    }
}