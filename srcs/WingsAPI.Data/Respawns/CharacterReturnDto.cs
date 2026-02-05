using ProtoBuf;

namespace WingsEmu.DTOs.Respawns;

[ProtoContract]
public class CharacterReturnDto
{
    [ProtoMember(1)]
    public short MapId { get; set; }

    [ProtoMember(2)]
    public short MapX { get; set; }

    [ProtoMember(3)]
    public short MapY { get; set; }
}