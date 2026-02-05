using System;
using ProtoBuf;

namespace WingsEmu.DTOs.Quests;

[ProtoContract]
public class CompletedScriptsDto
{
    [ProtoMember(1)]
    public int ScriptId { get; set; }

    [ProtoMember(2)]
    public int ScriptIndex { get; set; }

    [ProtoMember(3)]
    public DateTime CompletedDate { get; set; }
}