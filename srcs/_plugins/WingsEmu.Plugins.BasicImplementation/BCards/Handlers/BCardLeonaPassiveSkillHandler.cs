// NosEmu
// 


using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardLeonaPassiveSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardLeonaPassiveSkillHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.LeonaPassiveSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;
        byte subType = ctx.BCard.SubType;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        if (!sender.IsAlive())
        {
            return;
        }

        switch ((AdditionalTypes.LeonaPassiveSkill)subType)
        {
            case AdditionalTypes.LeonaPassiveSkill.OnSPWearCausing:

                if (sender is IMateEntity mateEntity && mateEntity.MonsterVNum == (short)MonsterVnum.LEONA)
                {
                    int randomNumber = _randomGenerator.RandomNumber();
                    Buff buff = _buffFactory.CreateBuff(secondDataValue, sender);

                    if (randomNumber > firstDataValue)
                    {
                        return;
                    }

                    sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
        }
    }
}