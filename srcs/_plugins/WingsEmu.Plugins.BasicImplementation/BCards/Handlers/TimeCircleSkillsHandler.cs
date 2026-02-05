using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class TimeCircleSkillsHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public TimeCircleSkillsHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.TimeCircleSkills;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.TimeCircleSkills.GatherEnergy:
                sender.ChargeComponent.SetCharge(bCard.FirstDataValue(sender.Level));
                sender.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.CHARGE, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}