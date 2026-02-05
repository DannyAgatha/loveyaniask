// NosEmu
// 


using WingsEmu.Packets.Enums;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("up_gr")]
    public class UpgradePacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public byte UpgradeType { get; set; }

        [PacketIndex(1)]
        public byte Data { get; set; }

        [PacketIndex(2)]
        public short Data2 { get; set; }

        [PacketIndex(3)]
        public short? Data3 { get; set; }

        [PacketIndex(4)]
        public short? Data4 { get; set; }

        [PacketIndex(5)]
        public short? Data5 { get; set; }

        [PacketIndex(6)]
        public short? Data6 { get; set; }

        #endregion
    }
}