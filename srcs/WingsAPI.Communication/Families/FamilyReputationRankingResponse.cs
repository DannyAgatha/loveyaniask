using ProtoBuf;

namespace WingsAPI.Communication.Families;

[ProtoContract]
public class FamilyReputationRankingResponse
{
    [ProtoMember(1)]
    public long Reputation { get; set; }
}