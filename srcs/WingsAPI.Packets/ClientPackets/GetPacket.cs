// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("get")]
    public class GetPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte PickerType { get; set; }

        [PacketIndex(1)]
        public int PickerId { get; set; }

        [PacketIndex(2)]
        public long TransportId { get; set; }

        #endregion
    }
}