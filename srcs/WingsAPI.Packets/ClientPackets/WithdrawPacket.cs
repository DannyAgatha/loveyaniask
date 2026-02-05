// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("withdraw")]
    public class WithdrawPacket : ClientPacket
    {
        [PacketIndex(0)]
        public short Slot { get; set; }

        [PacketIndex(1)]
        public int Amount { get; set; }

        [PacketIndex(2)]
        public bool PetBackpack { get; set; }
    }
}