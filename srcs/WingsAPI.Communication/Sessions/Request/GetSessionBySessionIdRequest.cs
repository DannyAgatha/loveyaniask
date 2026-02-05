using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request;

[ProtoContract]
public class GetSessionBySessionIdRequest
{
    [ProtoMember(1)]
    public string SessionId { get; init; }
}