using System;
using PhoenixLib.DAL;
using ProtoBuf;

namespace WingsAPI.Data.Account;

public class RefreshTokensDto : ILongDto
{
    [ProtoMember(2)]
    public string Token { get; set; }
    
    [ProtoMember(3)]
    public DateTime Expires { get; set; }
    
    [ProtoMember(4)]
    public bool IsValid { get; set; }
    
    [ProtoMember(1)]
    public long Id { get; set; }
}