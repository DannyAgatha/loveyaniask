using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families;

[ProtoContract]
public class FamilyAllResponse
{
    [ProtoMember(1)]
    public IEnumerable<FamilyDTO> Families { get; set; }
}