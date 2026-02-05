namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("script")]
    public class ScriptPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Type { get; set; }

        [PacketIndex(1, serializeToEnd: true)]
        public string Data { get; set; }

        #endregion
    }
}