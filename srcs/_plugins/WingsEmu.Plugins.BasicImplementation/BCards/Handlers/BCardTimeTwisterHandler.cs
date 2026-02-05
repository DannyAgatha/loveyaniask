using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardTimeTwisterHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IRandomGenerator _randomGenerator;

    public BCardTimeTwisterHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.AbsorbedSpirit;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;

        if (_randomGenerator.RandomNumber() > firstData)
        {
            return;
        }

        switch (subType)
        {
            case (byte)AdditionalTypes.AbsorbedSpirit.ApplyEffectIfPresent:
                if (!sender.BuffComponent.HasBuff((short)BuffVnums.SPIRIT_ABSORPTION))
                {
                    return;
                }

                sender.AddBuffAsync(_buffFactory.CreateBuff(secondData, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;

            case (byte)AdditionalTypes.AbsorbedSpirit.ApplyEffectIfNotPresent:
                if (sender.BuffComponent.HasBuff((short)BuffVnums.SPIRIT_ABSORPTION))
                {
                    return;
                }

                sender.AddBuffAsync(_buffFactory.CreateBuff(secondData, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}