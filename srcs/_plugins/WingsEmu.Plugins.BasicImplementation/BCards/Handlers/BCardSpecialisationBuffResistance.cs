// NosEmu
// 


using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialisationBuffResistance : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardSpecialisationBuffResistance(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public BCardType HandledType => BCardType.SpecialisationBuffResistance;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;
        int randomNumber = _randomGenerator.RandomNumber();

        if (randomNumber > firstData)
        {
            return;
        }

        switch (subType)
        {
            case (byte)AdditionalTypes.SpecialisationBuffResistance.RemoveBadEffects:
            {
                target.RemoveNegativeBuffs(secondData);
                break;
            }

            case (byte)AdditionalTypes.SpecialisationBuffResistance.RemoveGoodEffects:
            {
                target.RemovePositiveBuffs(secondData);
                break;
            }
        }
    }
}