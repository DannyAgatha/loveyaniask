using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDefencePiercingHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IBuffFactory _buffFactory;

    public BCardDefencePiercingHandler(IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.DefencePiercing;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.DefencePiercing.TargetReceiveBuffWhileHavingBuff:
                if (!sender.BuffComponent.HasBuff(firstData))
                {
                    return;
                }
                
                if (target.IsSameEntity(sender))
                {
                    return;
                }

                target.AddBuffAsync(_buffFactory.CreateBuff(secondData, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}