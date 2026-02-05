// NosEmu
// 


using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quicklist;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Quicklist;

public class QuicklistAddEventHandler : IAsyncEventProcessor<QuicklistAddEvent>
{
    public async Task HandleAsync(QuicklistAddEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        short tab = e.Tab;
        short slot = e.Slot;
        int morphId = session.PlayerEntity.UseSp ? session.PlayerEntity.Specialist?.GameItem.Morph ?? 0 : 0;
        
        if ((session.PlayerEntity.Class == ClassType.MartialArtist && session.PlayerEntity.Morph == (byte)MorphType.DraconicFistTransformed) || 
            session.PlayerEntity.Morph == (byte)MorphType.DraconicFist)
        {
            morphId = (byte)MorphType.DraconicFistTransformed;
        }
        
        CharacterQuicklistEntryDto from = session.PlayerEntity.QuicklistComponent.GetQuicklistByTabSlotAndMorph(tab, slot, morphId);

        if (from != null)
        {
            session.PlayerEntity.QuicklistComponent.RemoveQuicklist(tab, slot, morphId);
            session.SendEmptyQuicklistSlot(tab, slot);
        }

        // Used only for combo skills :sadge:
        IBattleEntitySkill? getSkill = e.DestinationType != 3 ? session.PlayerEntity.GetSkills().FirstOrDefault(x => x != null && x.Skill.Id > 200 && x.Skill.CastId == e.DestinationSlotOrVnum) : null;

        from = new CharacterQuicklistEntryDto
        {
            Morph = (short)morphId,
            Type = e.Type,
            QuicklistSlot = slot,
            QuicklistTab = tab,
            InventoryTypeOrSkillTab = e.DestinationType,
            InvSlotOrSkillSlotOrSkillVnum = e.DestinationSlotOrVnum,
            SkillVnum = (short?)getSkill?.Skill.Id
        };

        session.PlayerEntity.QuicklistComponent.AddQuicklist(from);
        session.SendQuicklistSlot(from);
    }
}