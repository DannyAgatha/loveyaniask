// NosEmu
// 


using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHPMPHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public BCardHPMPHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public BCardType HandledType => BCardType.HPMP;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;

        if (sender?.MapInstance == null)
        {
            return;
        }

        if (target?.MapInstance == null)
        {
            return;
        }

        BCardDTO bCard = ctx.BCard;
        int firstDataValue = bCard.FirstDataValue(sender.Level);

        if (!target.IsAlive())
        {
            return;
        }

        if (target.IsMateTrainer())
        {
            return;
        }

        switch (subType)
        {
            case (byte)AdditionalTypes.HPMP.RestoreDecreasedHP:

                if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int lostHp = target.MaxHp - target.Hp;
                
                if (lostHp <= 0)
                {
                    return;
                }
                
                int heal = (int)(lostHp * firstDataValue * 0.01);
                
                target.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = target,
                    HpHeal = heal
                });

                break;
            case (byte)AdditionalTypes.HPMP.DecreaseRemainingHP:
                int damage = (int)(target.Hp * firstDataValue * 0.01);

                if (target.Hp - damage <= 0)
                {
                    if (target.Hp != 1)
                    {
                        target.BroadcastDamage(target.Hp - 1);
                    }

                    target.Hp = 1;
                }
                else
                {
                    target.BroadcastDamage(damage);
                    target.Hp -= damage;
                }

                break;
            
            case (byte)AdditionalTypes.HPMP.RestoreDecreasedMP:

                if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }

                int lostMp = target.MaxMp - target.Mp;
                if (lostMp <= 0)
                {
                    return;
                }

                int healMp = (int)(lostMp * firstDataValue * 0.01);

                target.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = target,
                    MpHeal = healMp
                });

                break;
            
            case (byte)AdditionalTypes.HPMP.DecreaseRemainingMP:

                int mpDecrease = (int)(target.Mp * firstDataValue * 0.01);
                target.Mp = target.Mp - mpDecrease <= 0 ? 1 : target.Mp - mpDecrease;
                break;
        }

        if (target is not IPlayerEntity targetPlayer)
        {
            return;
        }

        targetPlayer.Session?.RefreshStat();
    }
}