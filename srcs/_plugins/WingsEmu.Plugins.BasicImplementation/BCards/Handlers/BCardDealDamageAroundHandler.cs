using PhoenixLib.Events;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDealDamageAroundHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public BCardDealDamageAroundHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.DealDamageAround;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.DealDamageAround.DealAreaDamagePerSecond:
            {
                IEnumerable<IBattleEntity> enemies = sender.GetEnemiesInRange(sender, (byte)firstDataValue).ToList();
                int damage = secondDataValue;
                foreach (IBattleEntity entity in enemies)
                {
                    if (!entity.IsAlive())
                    {
                        continue;
                    }

                    if (entity.Hp - damage <= 0)
                    {
                        entity.Hp = 1;
                        entity.BroadcastDamage(damage);
                    }
                    else
                    {
                        entity.BroadcastDamage(damage);
                        entity.Hp -= damage;
                    }
                }
            }
                break;
            case (byte)AdditionalTypes.DealDamageAround.EffectiveOnEnemyInAreaPerSecond:
                {
                    IEnumerable<IBattleEntity> enemies = sender.GetEnemiesInRange(sender, (byte)firstDataValue).ToList();

                    foreach (IBattleEntity entity in enemies)
                    {
                        if (!entity.IsAlive())
                        {
                            continue;
                        }

                        if (entity.BuffComponent.HasBuff(secondDataValue))
                        {
                            continue;
                        }

                        entity.AddBuffAsync(_buffFactory.CreateBuff(secondDataValue, entity)).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
                break;
            case (byte)AdditionalTypes.DealDamageAround.EffectiveOnAllieInAreaPerSecond:
            {
                IEnumerable<IBattleEntity> allies = sender.GetAlliesInRange(sender, (byte)firstDataValue).ToList();

                foreach (IBattleEntity entity in allies)
                {
                    if (!entity.IsAlive())
                    {
                        continue;
                    }

                    if (entity.BuffComponent.HasBuff(secondDataValue))
                    {
                        continue;
                    }

                    entity.AddBuffAsync(_buffFactory.CreateBuff(secondDataValue, entity)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
                break;
        }
    }
}