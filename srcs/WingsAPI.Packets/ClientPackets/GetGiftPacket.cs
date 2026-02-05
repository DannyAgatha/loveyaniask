// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("pcl")]
    public class GetGiftPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public int GiftId { get; set; }

        #endregion
    }
}