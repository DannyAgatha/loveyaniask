using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardKingOfTheBeastHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.KingOfTheBeast;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.KingOfTheBeast.HealthIncreaseOnDodge:
                {
                    if (target.IsSameEntity(sender))
                    {
                        return;
                    }

                    sender.EmitEventAsync(new BattleEntityHealEvent
                    {
                        Entity = sender,
                        HpHeal = firstData
                    });
                }

                break;
            
            case (byte)AdditionalTypes.KingOfTheBeast.MissingHPIncreasedOnDodge:
            {
                if (target.IsSameEntity(sender))
                {
                    return;
                }

                int missingHp = 100 - sender.GetHpPercentage();

                int heal = missingHp * firstData;

                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = heal
                });
            }

                break;
        }
    }
}