// NosEmu
// 


using ProtoBuf;

namespace WingsEmu.DTOs.Quicklist;

[ProtoContract]
public class CharacterQuicklistEntryDto
{
    [ProtoMember(1)]
    public short Morph { get; set; }

    [ProtoMember(2)]
    public short InvSlotOrSkillSlotOrSkillVnum { get; set; }

    [ProtoMember(3)]
    public short QuicklistTab { get; set; }

    [ProtoMember(4)]
    public short QuicklistSlot { get; set; }

    /// <summary>
    ///     SkillTabs:
    ///     0 => passive
    ///     1 => skills
    ///     2 => upgraded skills
    ///     3 => motion/emotes
    /// </summary>
    [ProtoMember(5)]
    public short InventoryTypeOrSkillTab { get; set; }

    [ProtoMember(6)]
    public QuicklistType Type { get; set; }

    [ProtoMember(7)]
    public short? SkillVnum { get; set; }
}

public enum QuicklistType
{
    ITEM = 0,
    SKILLS = 1
}