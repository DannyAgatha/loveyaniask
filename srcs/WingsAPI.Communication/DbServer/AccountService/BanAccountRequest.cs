using ProtoBuf;

namespace WingsAPI.Communication.DbServer.AccountService;

[ProtoContract]
public class BanAccountRequest
{
    [ProtoMember(1)]
    public long AccountId { get; init; }
    
    [ProtoMember(2)]
    public string Reason { get; init; }
}