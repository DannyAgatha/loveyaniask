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

public class BCardHealingBurningAndCastingHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public BCardHealingBurningAndCastingHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public BCardType HandledType => BCardType.HealingBurningAndCasting;

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
            case (byte)AdditionalTypes.HealingBurningAndCasting.RestoreHP:
            case (byte)AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                
                if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                target.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = target,
                    HpHeal = firstDataValue
                });

                break;
            case (byte)AdditionalTypes.HealingBurningAndCasting.RestoreMP:
                
                if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                if (target.Mp + firstDataValue < target.MaxMp)
                {
                    target.Mp += firstDataValue;
                }
                else
                {
                    target.Mp = target.MaxMp;
                }

                break;
            case (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseHP:

                if (target.Hp - firstDataValue <= 0)
                {
                    if (target.Hp != 1)
                    {
                        target.BroadcastDamage(target.Hp - 1);
                    }

                    target.Hp = 1;
                }
                else
                {
                    target.BroadcastDamage(firstDataValue);
                    target.Hp -= firstDataValue;
                }

                break;
            case (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseMP:
                target.Mp = target.Mp - firstDataValue <= 0 ? 1 : target.Mp - firstDataValue;
                break;
        }

        if (target is not IPlayerEntity targetPlayer)
        {
            return;
        }

        targetPlayer.Session?.RefreshStat();
    }
}