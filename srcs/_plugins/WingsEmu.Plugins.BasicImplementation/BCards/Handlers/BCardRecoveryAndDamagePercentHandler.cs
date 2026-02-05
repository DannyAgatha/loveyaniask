// NosEmu
// 
// Developed by NosWings Team

using System;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardRecoveryAndDamagePercentHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.RecoveryAndDamagePercent;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        sender ??= target;

        BCardDTO bCard = ctx.BCard;
        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);
        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPRecovered:
                {
                    if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                    {
                        return;
                    }
                    
                    int heal = (int)(target.MaxHp * (firstDataValue * 0.01));
                    target.EmitEvent(new BattleEntityHealEvent
                    {
                        Entity = target,
                        HpHeal = heal
                    });
                }
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP:
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPReduced:
            {
                int damage = (int)(target.Hp * (firstDataValue * 0.01));
                if (target.Hp - damage <= 1)
                {
                    damage = Math.Abs(target.Hp - 1);
                }

                target.Hp -= damage;
                if (damage == 0)
                {
                    return;
                }

                target.BroadcastDamage(damage);
            }
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.MPRecovered:
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int mpRegen = (int)(sender.MaxMp * (firstDataValue * 0.01));

                if (sender.Mp + mpRegen > sender.MaxMp)
                {
                    sender.Mp = sender.MaxMp;
                }
                else
                {
                    sender.Mp += mpRegen;
                }
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.MPReduced:

                if (target.IsPlayer() && sender.IsPlayer())
                {
                    if (target is IPlayerEntity player)
                    {
                        if (player.GetMaxArmorShellValue(ShellEffectType.ProtectMPInPVP) > 0)
                        {
                            return;
                        }
                    }
                }

                int mpDamage = (int)(target.MaxMp * (firstDataValue * 0.01));
                if (target.Mp - mpDamage <= 1)
                {
                    mpDamage = Math.Abs(target.Mp - 1);
                }

                target.Mp -= mpDamage;
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPIncreasedPerDebuff:
            {
                if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                if (target.BCardDataComponent.IncreaseHpPerDebuff.HasValue)
                {   
                    int heal = (int)(target.MaxHp * (firstDataValue * 0.01));
                    
                    if (target.BCardDataComponent.IncreaseHpPerDebuff < secondDataValue)
                    {
                        target.BCardDataComponent.IncreaseHpPerDebuff++;
                    }
                    
                    heal += heal * target.BCardDataComponent.IncreaseHpPerDebuff.Value;
                    target.EmitEvent(new BattleEntityHealEvent
                    {
                        Entity = target,
                        HpHeal = heal
                    });
                }
            }
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.HPDecreasedPerDebuff:
            {
                if (target.BCardDataComponent.DecreaseHpPerDebuff.HasValue)
                {   
                    int damage = (int)(target.MaxHp * (firstDataValue * 0.01));
                    
                    if (target.BCardDataComponent.DecreaseHpPerDebuff < secondDataValue)
                    {
                        target.BCardDataComponent.DecreaseHpPerDebuff++;
                    }
                    
                    damage += damage * target.BCardDataComponent.DecreaseHpPerDebuff.Value;

                    if (target.Hp - damage <= 1)
                    {
                        damage = Math.Abs(target.Hp - 1);
                    }

                    target.Hp -= damage;
                    if (damage == 0)
                    {
                        return;
                    }

                    target.BroadcastDamage(damage);
                }
            }
                break;
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.EnemyHPReducedCasterRegen:
            {
                int damage = firstDataValue;

                if (target.Hp - damage <= 1)
                {
                    damage = Math.Abs(target.Hp - 1);
                }

                target.Hp -= damage;
                if (damage == 0)
                {
                    return;
                }

                int heal = (int)(damage * (secondDataValue * 0.01));
                sender.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = heal
                });
                target.BroadcastDamage(damage);
            }
                break;
            
            case (byte)AdditionalTypes.RecoveryAndDamagePercent.EnemyMPReducedCasterRegen:
            {
                int damage = firstDataValue;

                if (target.Mp - damage <= 1)
                {
                    damage = Math.Abs(target.Mp - 1);
                }

                target.Mp -= damage;
                if (damage == 0)
                {
                    return;
                }

                int heal = (int)(damage * (secondDataValue * 0.01));
                sender.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = sender,
                    MpHeal = heal
                });
                target.BroadcastDamage(damage);
            }
                break;
        }

        if (target is not IPlayerEntity playerEntity)
        {
            return;
        }

        playerEntity.Session.RefreshStat();
    }
}