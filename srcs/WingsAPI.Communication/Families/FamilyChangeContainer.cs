using ProtoBuf;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyChangeContainer
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }

        [ProtoMember(2)]
        public FamilyAuthority RequestedFamilyAuthority { get; set; }
    }
}