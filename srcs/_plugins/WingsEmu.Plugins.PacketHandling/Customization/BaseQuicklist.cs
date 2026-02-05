// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Quicklist;

namespace WingsEmu.Customization.NewCharCustomisation;

public class BaseQuicklist
{
    public BaseQuicklist() => Quicklist = new List<CharacterQuicklistEntryDto>
    {
        new()
        {
            Type = QuicklistType.SKILLS,
            InventoryTypeOrSkillTab = 1,
            InvSlotOrSkillSlotOrSkillVnum = 1
        },
        new()
        {
            Type = QuicklistType.ITEM,
            QuicklistSlot = 1,
            InventoryTypeOrSkillTab = 2
        },
        new()
        {
            QuicklistSlot = 8,
            Type = QuicklistType.SKILLS,
            InventoryTypeOrSkillTab = 1,
            InvSlotOrSkillSlotOrSkillVnum = 16
        },
        new()
        {
            QuicklistSlot = 9,
            Type = QuicklistType.SKILLS,
            InventoryTypeOrSkillTab = 3,
            InvSlotOrSkillSlotOrSkillVnum = 1
        }
    };

    public List<CharacterQuicklistEntryDto> Quicklist { get; set; }
}