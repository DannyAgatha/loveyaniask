// NosEmu
// 


using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("fmg")]
    public class FamilyManagementPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public FamilyAuthority FamilyAuthorityType { get; set; }

        [PacketIndex(1)]
        public long TargetId { get; set; }

        [PacketIndex(2)]
        public byte? Confirmed { get; set; }

        #endregion
    }
}