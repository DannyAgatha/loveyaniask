using ProtoBuf;
using System.Collections.Generic;

namespace WingsAPI.Data.Prestige;

[ProtoContract]
public class CharacterPrestigeDto
{
    [ProtoMember(1)]
    public int CurrentPrestigeLevel { get; set; }

    [ProtoMember(2)]
    public long CurrentPrestigeExp { get; set; }

    [ProtoMember(3)]
    public List<PrestigeTaskDto> Tasks { get; set; } = [];
}