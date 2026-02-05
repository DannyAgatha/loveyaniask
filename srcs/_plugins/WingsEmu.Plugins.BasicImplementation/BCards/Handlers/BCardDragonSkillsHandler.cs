using System;
using System.Linq;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardDragonSkillsHandler : IBCardEffectAsyncHandler
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly ISkillsManager _skillManager;

        public BCardDragonSkillsHandler(IAsyncEventPipeline eventPipeline, ISkillsManager skillManager)
        {
            _eventPipeline = eventPipeline;
            _skillManager = skillManager;
        }

        public BCardType HandledType => BCardType.DragonSkills;

        public async void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity sender = ctx.Sender;
            IBattleEntity target = ctx.Target;        
            SkillInfo skillInfo = ctx.Skill;
            int damageDealt = ctx.DamageDealt;
            int firstData = ctx.BCard.FirstDataValue(sender.Level);
            int secondData = ctx.BCard.SecondDataValue(sender.Level);
            
            switch (ctx.BCard.SubType)
            {
                case (byte)AdditionalTypes.DragonSkills.CooldownResetChance:
                case (byte)AdditionalTypes.DragonSkills.CooldownResetChanceOnSKill:

                    if (sender is not IPlayerEntity playerEntity)
                    {
                        return;
                    }
                    
                    if (!sender.IsSucceededChance(firstData))
                    {
                        return;
                    }

                    SkillInfo skillToReset = _skillManager.GetSkill(secondData).GetInfo();
                    playerEntity.ClearSkillCooldownsById(skillToReset.CastId);
                    IBattleEntitySkill skill = playerEntity.Skills.FirstOrDefault(s => s.Skill.CastId == skillToReset.CastId);
                    
                    if (skill != null && (sender.BCardComponent.HasBCard(BCardType.SpecialDamageAndExplosions, (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR) ||
                            skill.Skill.BCards.Any(b => b.Type == (int)BCardType.SpecialDamageAndExplosions && b.SubType == (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR)))
                    {
                        return;
                    }
                    
                    if (skill != null)
                    {
                        skill.LastUse = DateTime.MinValue;
                    }

                    playerEntity.Session.SendSkillCooldownReset(skillToReset.CastId);
                    break;
                
                case (byte)AdditionalTypes.DragonSkills.ChangeIntoHaetae:
                case (byte)AdditionalTypes.DragonSkills.ChangeIntoDragon:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    if (player.Morph == (byte)MorphType.DraconicFist && !ctx.BCard.CardId.HasValue)
                    {
                        break;
                    }

                    if (player.Morph == (byte)MorphType.DraconicFistTransformed)
                    {
                        player.RemoveBuffAsync((int)BuffVnums.TRANSFORMATION_DRAGON, true).ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    }

                    player.Morph = (byte)MorphType.DraconicFistTransformed;
                    player.IsDraconicMorphed = true;
                    player.Session.BroadcastCMode();
                    player.Session.BroadcastEffect(EffectType.Transform);
                    player.Session.SendCancelPacket(CancelType.InCombatMode);
                }
                break;
                
                case (byte)AdditionalTypes.DragonSkills.MagicArrowChance:
                {
                    if (damageDealt is 0)
                    {
                        return;
                    }

                    if (skillInfo.AttackType != AttackType.Ranged && skillInfo.AttackType != AttackType.Magical)
                    {
                        return;
                    }

                    if (!sender.IsSucceededChance(firstData))
                    {
                        return;
                    }

                    int magicArrowDamage = (int)(damageDealt * 0.15);
                    await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                    {
                        Damaged = target,
                        Damager = sender,
                        Damage = magicArrowDamage,
                        CanKill = false,
                        SkillInfo = skillInfo
                    });
                    sender.BroadcastZephyrPacket(target, skillInfo, magicArrowDamage);
                }
                    break;
            }
        }
    }
}