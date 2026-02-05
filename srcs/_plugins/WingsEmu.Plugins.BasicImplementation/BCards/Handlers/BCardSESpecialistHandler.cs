using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSESpecialistHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public BCardSESpecialistHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.SESpecialist;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.SESpecialist.EnterNumberOfBuffsAndDamage:
                bool alreadyHaveBuffDamage = sender.EndBuffDamages.Any(x => x.Key == firstDataValue);
                if (alreadyHaveBuffDamage)
                {
                    sender.RemoveEndBuffDamage((short)firstDataValue);
                }

                sender.AddEndBuff((short)firstDataValue, secondDataValue * 1000);
                break;

            case (byte)AdditionalTypes.SESpecialist.LowerHPStrongerEffect:
                if (sender.GetHpPercentage() <= 30)
                {
                    Buff lifeAndDeath3 = _buffFactory.CreateBuff((short)BuffVnums.LIFE_AND_DEATH_3, sender);
                    sender.AddBuffAsync(lifeAndDeath3).GetAwaiter().GetResult();;
                }
                else if (sender.GetHpPercentage() <= 50)
                {
                    Buff lifeAndDeath2 = _buffFactory.CreateBuff((short)BuffVnums.LIFE_AND_DEATH_2, sender);
                    sender.AddBuffAsync(lifeAndDeath2).GetAwaiter().GetResult();;
                }
                else
                {
                    Buff lifeAndDeath = _buffFactory.CreateBuff((short)BuffVnums.LIFE_AND_DEATH, sender);
                    sender.AddBuffAsync(lifeAndDeath).GetAwaiter().GetResult();;
                }
                break;
        }
    }
}