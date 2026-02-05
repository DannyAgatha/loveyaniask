// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("shopping")]
    public class ShoppingPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(3)]
        public int NpcId { get; set; }

        #endregion
    }
}