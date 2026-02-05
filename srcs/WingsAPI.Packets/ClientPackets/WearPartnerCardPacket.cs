namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("wear_pc")]
    public class WearPartnerCardPacket : ClientPacket
    {
        [PacketIndex(0)]
        public byte InventorySlot { get; set; }

        [PacketIndex(1)]
        public byte PetId { get; set; }
    }
}