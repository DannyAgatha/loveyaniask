using PhoenixLib.Events;
using System;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardInflictSkillHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.InflictSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skillInfo = ctx.Skill;
        BCardDTO bCard = ctx.BCard;
        int firstData = bCard.FirstDataValue(sender.Level);

        switch (bCard.SubType)
        {

            case (byte)AdditionalTypes.InflictSkill.RageBarIncreased:
            case (byte)AdditionalTypes.InflictSkill.RageBarIncreasedTick:
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                player.LastEnergyRefill = DateTime.UtcNow;
                player.UpdateEnergyBar(firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.InflictSkill.RageBarDecreased:
            case (byte)AdditionalTypes.InflictSkill.RageBarDecreasedTick:
                if (sender is not IPlayerEntity character)
                {
                    return;
                }

                character.LastEnergyRefill = DateTime.UtcNow;
                character.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            
            case (byte)AdditionalTypes.InflictSkill.OnDefenceResetCooldownOfSkillUsed:
            {
                if (!sender.IsSucceededChance(firstData))
                {
                    return;
                }

                if (target is not IPlayerEntity playerEntity)
                {
                    return;
                }

                if (skillInfo is null)
                {
                    return;
                }

                if (skillInfo.CastId is 0)
                {
                    return;
                }

                playerEntity.ClearSkillCooldownsById(skillInfo.CastId);
                IBattleEntitySkill skill = playerEntity.Skills.FirstOrDefault(s => s.Skill.CastId == skillInfo.CastId);
                if (skill != null)
                {
                    skill.LastUse = DateTime.MinValue;
                    playerEntity.Session.SendSkillCooldownReset(skill.Skill.CastId);
                }
            }
                break;
        }
    }
}