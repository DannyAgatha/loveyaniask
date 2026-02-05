using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families;

[ProtoContract]
public class FamilyBuffCreateRequest
{
    [ProtoMember(1)]
    public FamilyBuffCrossChannel FamilyBuffsCrossChannel { get; set; }
}