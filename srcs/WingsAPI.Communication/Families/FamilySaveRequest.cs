using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families;

[ProtoContract]
public class FamilySaveRequest
{
    [ProtoMember(1)]
    public FamilyDTO FamilyDto { get; set; }
}