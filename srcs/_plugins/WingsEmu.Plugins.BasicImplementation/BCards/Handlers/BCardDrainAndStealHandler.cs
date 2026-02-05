// NosEmu
// 


using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDrainAndStealHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardDrainAndStealHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public BCardType HandledType => BCardType.DrainAndSteal;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;
        byte subType = ctx.BCard.SubType;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        if (ctx.Skill?.Vnum is (short)SkillsVnums.IMMORTALITY)
        {
            if (ctx.Skill.PartnerSkillRank != null)
            {
                secondDataValue = bCard.SecondDataValue((int)ctx.Skill.PartnerSkillRank);
            }
        }

        if (!target.IsAlive() || !sender.IsAlive())
        {
            return;
        }

        if (target.IsMateTrainer())
        {
            return;
        }

        switch (subType)
        {
            case (byte)AdditionalTypes.DrainAndSteal.GiveEnemyHP:
                break;
            case (byte)AdditionalTypes.DrainAndSteal.LeechEnemyHP:
                if (_randomGenerator.RandomNumber() > firstDataValue)
                {
                    return;
                }

                if (target.Hp - secondDataValue > 0)
                {
                    target.BroadcastDamage(secondDataValue);
                    target.Hp -= secondDataValue;
                }

                if (sender.Hp + secondDataValue < sender.MaxHp)
                {
                    if (target.Hp - secondDataValue <= 0)
                    {
                        target.Hp = 1;
                    }
                    
                    sender.Hp += secondDataValue;
                }
                else
                {
                    if (target.Hp - secondDataValue <= 0)
                    {
                        target.Hp = 1;
                    }
                    sender.Hp = sender.MaxHp;
                }

                (target as IPlayerEntity)?.Session.RefreshStat();
                (sender as IPlayerEntity)?.Session.RefreshStat();
                break;
            case (byte)AdditionalTypes.DrainAndSteal.GiveEnemyMP:
                break;
            case (byte)AdditionalTypes.DrainAndSteal.LeechEnemyMP:
                if (_randomGenerator.RandomNumber() > firstDataValue)
                {
                    return;
                }
                
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

                if (target.Mp - secondDataValue > 0)
                {
                    target.BroadcastDamage(secondDataValue, DmType.DamageBlue);
                    target.Mp -= secondDataValue;
                }

                if (sender.Mp + secondDataValue < sender.MaxMp)
                {
                    if (target.Mp - secondDataValue <= 0)
                    {
                        target.Mp = 1;
                    }

                    sender.Mp += secondDataValue;
                }
                else
                {
                    if (target.Mp - secondDataValue <= 0)
                    {
                        target.Mp = 1;
                    }

                    sender.Mp = sender.MaxMp;
                }

                (target as IPlayerEntity)?.Session.RefreshStat();
                (sender as IPlayerEntity)?.Session.RefreshStat();
                break;
            case (byte)AdditionalTypes.DrainAndSteal.ConvertEnemyMPToHP:
                break;
            case (byte)AdditionalTypes.DrainAndSteal.ConvertEnemyHPToMP:

                int toRemoveAndAdd = firstDataValue;

                if (sender.Mp + toRemoveAndAdd > sender.MaxMp)
                {
                    if (target.Hp - toRemoveAndAdd <= 0)
                    {
                        target.BroadcastDamage(target.Hp - 1);
                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(toRemoveAndAdd);
                        target.Hp -= toRemoveAndAdd;
                        sender.EmitEvent(new BattleEntityHealEvent
                        {
                            Entity = sender,
                            MpHeal = sender.MaxMp
                        });
                    }
                }
                else
                {
                    if (target.Hp - toRemoveAndAdd <= 0)
                    {
                        toRemoveAndAdd = target.Hp - 1;
                        target.BroadcastDamage(toRemoveAndAdd);
                        target.Hp = 1;
                    }
                    else
                    {
                        target.BroadcastDamage(toRemoveAndAdd);
                        target.Hp -= toRemoveAndAdd;
                    }

                    sender.EmitEvent(new BattleEntityHealEvent
                    {
                        Entity = sender,
                        MpHeal = toRemoveAndAdd
                    });
                }

                (target as IPlayerEntity)?.Session.RefreshStat();
                (sender as IPlayerEntity)?.Session.RefreshStat();

                break;
        }
    }
}