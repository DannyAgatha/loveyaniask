// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("aa_slslot")]
    public class AASlSlotPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public int CardId { get; set; }

        [PacketIndex(1)]
        public byte Slot { get; set; }

        #endregion
    }
}