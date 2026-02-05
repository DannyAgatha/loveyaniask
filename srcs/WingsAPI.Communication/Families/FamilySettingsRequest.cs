using ProtoBuf;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilySettingsRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }

        [ProtoMember(2)]
        public FamilyAuthority Authority { get; set; }

        [ProtoMember(3)]
        public FamilyActionType FamilyActionType { get; set; }

        [ProtoMember(4)]
        public byte Value { get; set; }
    }
}