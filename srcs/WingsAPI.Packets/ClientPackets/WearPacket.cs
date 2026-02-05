// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("wear")]
    public class WearPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte InventorySlot { get; set; }

        [PacketIndex(1)]
        public byte PetId { get; set; }

        [PacketIndex(2)]
        public bool BoundItem { get; set; }

        #endregion
    }
}