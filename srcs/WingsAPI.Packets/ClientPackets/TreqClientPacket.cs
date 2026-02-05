namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("treq")]
    public class TreqClientPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int X { get; set; }

        [PacketIndex(1)]
        public int Y { get; set; }

        [PacketIndex(2)]
        public byte StartPress { get; set; }

        [PacketIndex(3)]
        public long RecordPressAndCharacterId { get; set; }

        [PacketIndex(4)]
        public bool RecordPressConfirm { get; set; }

        #endregion
    }
}