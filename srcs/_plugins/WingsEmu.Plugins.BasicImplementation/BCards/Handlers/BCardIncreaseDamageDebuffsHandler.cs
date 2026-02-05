using System;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardIncreaseDamageDebuffsHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardIncreaseDamageDebuffsHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
    }
    
    public BCardType HandledType => BCardType.IncreaseDamageDebuffs;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        switch ((AdditionalTypes.IncreaseDamageDebuffs)ctx.BCard.SubType)
        {
            case AdditionalTypes.IncreaseDamageDebuffs.ProvideChanceInflictByMissingHP:
                if (target == null || !target.IsAlive())
                {
                    return;
                }

                if (sender.IsSameEntity(target))
                {
                    return;
                }
                
                double chance = Math.Floor((sender.MaxHp - sender.Hp) * (double)bCard.FirstData / sender.MaxHp);

                if (_randomGenerator.RandomNumber() > chance)
                {
                    return;
                }

                Buff buff = _buffFactory.CreateBuff(bCard.SecondData, sender);

                target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            
            case AdditionalTypes.IncreaseDamageDebuffs.EarnUltimatePointsOnSuccessfulAttack when sender is IPlayerEntity player:
                player.UpdateEnergyBar(bCard.FirstData).ConfigureAwait(false).GetAwaiter().GetResult();
                break;

            case AdditionalTypes.IncreaseDamageDebuffs.CanUseUltimateSkills when sender is IPlayerEntity player:
                player.Session.SendSpFtptPacket();
                player.Session.RefreshQuicklist(true);
                break;
        }
    }
}