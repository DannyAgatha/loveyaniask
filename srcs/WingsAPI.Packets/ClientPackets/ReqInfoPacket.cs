// NosEmu
// 


namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("req_info")]
    public class ReqInfoPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(1)]
        public long TargetVNum { get; set; }

        [PacketIndex(2)]
        public int? MateVNum { get; set; }

        #endregion
    }
}