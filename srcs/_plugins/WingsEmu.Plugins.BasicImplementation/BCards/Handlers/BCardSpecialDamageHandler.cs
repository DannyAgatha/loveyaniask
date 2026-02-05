using System.Collections.Generic;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialDamageHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardSpecialDamageHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public BCardType HandledType => BCardType.SpecialDamageAndExplosions;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;

        if (sender == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        if (sender.MapInstance?.Id != target.MapInstance?.Id)
        {
            return;
        }

        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.SpecialDamageAndExplosions.ChanceExplosion:
                if (!target.IsAlive())
                {
                    return;
                }

                if (_randomGenerator.RandomNumber() > firstData)
                {
                    return;
                }

                if (target.Hp - secondData <= 0)
                {
                    target.Hp = 1;
                }
                else
                {
                    target.Hp -= secondData;
                }

                target.BroadcastEffectInRange(EffectType.FireExplosion);
                if (target is not IPlayerEntity playerEntity)
                {
                    return;
                }

                playerEntity.Session.RefreshStat();
                break;
            case (byte)AdditionalTypes.SpecialDamageAndExplosions.ExplosionCauses:

                IEnumerable<IBattleEntity> targets = sender.GetEnemiesInRange(sender, (byte)firstData);

                foreach (IBattleEntity skillTarget in targets)
                {
                    skillTarget.BroadcastEffectInRange(EffectType.SmokePuff);
                    if (skillTarget.Hp - secondData <= 0)
                    {
                        skillTarget.BroadcastDamage(skillTarget.Hp - 1);
                        skillTarget.Hp = 1;
                    }
                    else
                    {
                        skillTarget.Hp -= secondData;
                        skillTarget.BroadcastDamage(secondData);
                    }

                    if (skillTarget is not IPlayerEntity player)
                    {
                        continue;
                    }

                    player.Session.RefreshStat();
                }

                break;
            case (byte)AdditionalTypes.SpecialDamageAndExplosions.ExplosionCausesNegated:

                targets = target.GetAlliesInRange(target, (byte)firstData);

                foreach (IBattleEntity skillTarget in targets)
                {
                    skillTarget.BroadcastEffectInRange(EffectType.FireExplosion);
                    if (skillTarget.Hp - secondData <= 0)
                    {
                        skillTarget.BroadcastDamage(skillTarget.Hp - 1);
                        skillTarget.Hp = 1;
                    }
                    else
                    {
                        skillTarget.Hp -= secondData;
                        skillTarget.BroadcastDamage(secondData);
                    }

                    if (skillTarget is not IPlayerEntity player)
                    {
                        continue;
                    }

                    player.Session.RefreshStat();
                }

                break;
        }
    }
}