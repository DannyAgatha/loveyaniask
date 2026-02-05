using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDropItemTwiceSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IRandomGenerator _randomGenerator;

    public BCardDropItemTwiceSkillHandler(IBuffFactory buffFactory, IRandomGenerator randomGenerator)
    {
        _buffFactory = buffFactory;
        _randomGenerator = randomGenerator;
    }

    public BCardType HandledType => BCardType.DropItemTwice;
    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.DropItemTwice.EffectOnSelfWhileAttackingChance:
            case (byte)AdditionalTypes.DropItemTwice.EffectOnSelfWhileDefendingChance:
            {
                int chance = bCard.FirstData;

                if (_randomGenerator.RandomNumber() > chance)
                {
                    return;
                }
                
                Buff buff = _buffFactory.CreateBuff(bCard.SecondData, sender);

                sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
            
            case (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileAttackingChance:
            case (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
            {
                if (target.IsSameEntity(sender))
                {
                    return;
                }
                
                int chance = bCard.FirstData;

                if (_randomGenerator.RandomNumber() > chance)
                {
                    return;
                }
                
                Buff buff = _buffFactory.CreateBuff(bCard.SecondData, sender);

                target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
        }
    }
}