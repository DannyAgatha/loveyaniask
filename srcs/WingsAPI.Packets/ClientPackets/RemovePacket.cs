// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("remove")]
    public class RemovePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte InventorySlot { get; set; }

        [PacketIndex(1)]
        public short PartnerSlot { get; set; }

        #endregion
    }
}