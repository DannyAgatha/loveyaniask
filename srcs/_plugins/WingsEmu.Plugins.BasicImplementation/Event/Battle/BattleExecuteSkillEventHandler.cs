using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.Event.Battle;

public class BattleExecuteSkillEventHandler : IAsyncEventProcessor<BattleExecuteSkillEvent>
{
    private readonly ISkillExecutor _skillExecutor;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    public BattleExecuteSkillEventHandler(ISkillExecutor skillExecutor, RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        _skillExecutor = skillExecutor;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
    }

    public async Task HandleAsync(BattleExecuteSkillEvent e, CancellationToken cancellation)
    {
        IBattleEntity caster = e.Entity;
        IBattleEntity target = e.Target;
        SkillInfo skillInfo = e.SkillInfo;
        DateTime endSkillCastTime = e.EndSkillCastTime;
        Position position = e.Position;
        
        caster.SetCastingSkill(skillInfo, endSkillCastTime);
        skillInfo.Cooldown = (short)caster.ApplyCooldownReduction(skillInfo);

        if (skillInfo.TargetType == TargetType.NonTarget)
        {
            switch (skillInfo.TargetAffectedEntities)
            {
                case TargetAffectedEntities.Enemies:
                    _skillExecutor.ExecuteDamageZoneHitSkill(caster, caster.SkillCast, position);
                    break;
                case TargetAffectedEntities.DebuffForEnemies:
                    _skillExecutor.ExecuteDebuffZoneHitSkill(caster, caster.SkillCast, position);
                    break;
                case TargetAffectedEntities.BuffForAllies:
                    _skillExecutor.ExecuteBuffZoneHitSkill(caster, caster.SkillCast, position);
                    break;
            }

            return;
        }

        switch (skillInfo.TargetAffectedEntities)
        {
            case TargetAffectedEntities.Enemies:
                _skillExecutor.ExecuteDamageSkill(caster, target, caster.SkillCast, position);
                break;
            case TargetAffectedEntities.BuffForAllies:
                _skillExecutor.ExecuteBuffSkill(caster, target, caster.SkillCast);
                break;
            case TargetAffectedEntities.DebuffForEnemies:
                switch (skillInfo.AttackType)
                {
                    case AttackType.Dash:
                        _skillExecutor.ExecuteDamageSkill(caster, target, caster.SkillCast, position);
                        break;
                    default:
                    {
                        if (!skillInfo.BCards.Any(x => x.Type == (short)BCardType.LordHatus &&
                                x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf))
                        {
                            _skillExecutor.ExecuteDebuffSkill(caster, target, caster.SkillCast);
                        }
                        else
                        {
                            _skillExecutor.ExecuteDamageSkill(caster, target, caster.SkillCast, position);
                        }

                        break;
                    }
                }
                break;
            case TargetAffectedEntities.None:
                if (caster is not IPlayerEntity character)
                {
                    caster.CancelCastingSkill();
                    return;
                }

                character.Session.SendSpecialistGuri(skillInfo.CastId);
                character.CancelCastingSkill();
                break;
            default:
                return;
        }
    }
}