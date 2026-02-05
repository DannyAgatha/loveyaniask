using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class DailyRewardDto
{
    [ProtoMember(1)]
    public string Time { get; set; }

    [ProtoMember(2)]
    public string IpAddress { get; set; }

    [ProtoMember(3)]
    public string Type { get; set; }
}