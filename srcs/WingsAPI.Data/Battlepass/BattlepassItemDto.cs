using ProtoBuf;

namespace WingsAPI.Data.BattlePass;

[ProtoContract]
public class BattlePassItemDto
{
    [ProtoMember(1)]
    public long BearingId { get; set; }

    [ProtoMember(2)]
    public bool IsPremium { get; set; }
}