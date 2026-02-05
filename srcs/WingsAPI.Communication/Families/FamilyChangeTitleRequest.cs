using ProtoBuf;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyChangeTitleRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }

        [ProtoMember(2)]
        public FamilyTitle RequestedFamilyTitle { get; set; }
    }
}