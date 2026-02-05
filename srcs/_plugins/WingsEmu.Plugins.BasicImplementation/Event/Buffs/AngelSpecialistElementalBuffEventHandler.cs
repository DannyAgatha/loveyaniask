using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;

namespace NosEmu.Plugins.BasicImplementations.Event.Buffs;

public class AngelSpecialistElementalBuffEventHandler : IAsyncEventProcessor<AngelSpecialistElementalBuffEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly ISkillsManager _skillsManager;

    public AngelSpecialistElementalBuffEventHandler(IBuffFactory buffFactory, ISkillsManager skillsManager)
    {
        _buffFactory = buffFactory;
        _skillsManager = skillsManager;
    }

    public async Task HandleAsync(AngelSpecialistElementalBuffEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;
        SkillInfo skillInfo = e.Skill;

        if (character.AngelElement.HasValue)
        {
            return;
        }

        if (!character.UseSp)
        {
            return;
        }

        if (character.Specialist == null)
        {
            return;
        }

        if (!character.HasBuff(BuffVnums.MAGICAL_FETTERS))
        {
            return;
        }

        character.RemoveAngelElement();
        await character.RemoveBuffAsync(false, character.BuffComponent.GetBuff((short)BuffVnums.MAGICAL_FETTERS));
        Buff newBuff = _buffFactory.CreateBuff((short)BuffVnums.MAGIC_SPELL, character);
        await character.AddBuffAsync(newBuff);

        if (character.Specialist.SpLevel < 20)
        {
            return;
        }

        short skillId = (ElementType)skillInfo.Element switch
        {
            ElementType.Neutral => 1195,
            ElementType.Fire => 1191,
            ElementType.Water => 1192,
            ElementType.Light => 1193,
            ElementType.Shadow => 1194
        };


        SkillDTO skill = _skillsManager.GetSkill(skillId);
        if (skill == null)
        {
            return;
        }

        var newComboState = new ComboSkillState
        {
            State = byte.MinValue,
            LastSkillByCastId = (byte)skill.CastId,
            OriginalSkillCastId = (byte)skill.CastId,
            AngelSkillVnumId = (short)skill.Id
        };

        character.SaveComboSkill(newComboState);
        session.SendMSlotPacket(skill.Id);
        character.AddAngelElement((ElementType)skillInfo.Element);
    }
}