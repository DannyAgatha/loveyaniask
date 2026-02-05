// NosEmu
// 


using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Packets.ClientPackets
{
    [PacketHeader("fauth")]
    public class FAuthPacket : ClientPacket
    {
        #region Properties

        [PacketIndex(0)]
        public FamilyAuthority MemberType { get; set; }

        [PacketIndex(1)]
        public byte AuthorityId { get; set; }

        [PacketIndex(2)]
        public byte Value { get; set; }

        #endregion
    }
}