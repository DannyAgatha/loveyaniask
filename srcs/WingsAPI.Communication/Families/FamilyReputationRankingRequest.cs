using ProtoBuf;

namespace WingsAPI.Communication.Families;

[ProtoContract]
public class FamilyReputationRankingRequest
{
    [ProtoMember(1)]
    public long FamilyId { get; set; }
}