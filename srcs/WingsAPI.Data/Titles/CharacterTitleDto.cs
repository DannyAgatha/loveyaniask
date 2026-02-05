// NosEmu
// 


using ProtoBuf;

namespace WingsEmu.DTOs.Titles;

[ProtoContract]
public class CharacterTitleDto
{
    [ProtoMember(1)]
    public int ItemVnum { get; set; }

    [ProtoMember(2)]
    public int TitleId { get; set; }

    [ProtoMember(3)]
    public bool IsVisible { get; set; }

    [ProtoMember(4)]
    public bool IsEquipped { get; set; }
}