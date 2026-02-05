// NosEmu
// 


using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDrainHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.Drain;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);

        switch (subType)
        {
            case (byte)AdditionalTypes.Drain.TransferEnemyHPNegated:
            case (byte)AdditionalTypes.Drain.TransferEnemyHP:
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                if (target.Hp - firstDataValue <= 0)
                {
                    if (target.Hp != 1)
                    {
                        target.BroadcastDamage(target.Hp);
                    }

                    target.Hp = 1;
                }
                else
                {
                    target.BroadcastDamage(firstDataValue);
                    target.Hp -= firstDataValue;
                }

                if (sender.MapInstance?.Id != target.MapInstance?.Id)
                {
                    return;
                }

                sender.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = firstDataValue
                });
                break;
        }
    }
}