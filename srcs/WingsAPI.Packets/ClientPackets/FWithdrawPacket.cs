// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("f_withdraw")]
    public class FWithdrawPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public short Slot { get; set; }

        [PacketIndex(1)]
        public int Amount { get; set; }

        [PacketIndex(2)]
        public byte? Unknown { get; set; }

        #endregion
    }
}