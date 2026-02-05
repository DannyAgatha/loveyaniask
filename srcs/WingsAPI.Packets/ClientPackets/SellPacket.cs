// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("sell")]
    public class SellPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(2)]
        public short Data { get; set; }

        [PacketIndex(3)]
        public byte? Slot { get; set; }

        [PacketIndex(4)]
        public ushort? Amount { get; set; }

        #endregion
    }
}