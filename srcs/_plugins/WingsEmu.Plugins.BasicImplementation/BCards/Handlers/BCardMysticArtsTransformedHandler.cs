using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMysticArtsTransformedHandler  : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    
    public BCardMysticArtsTransformedHandler (IBuffFactory buffFactory)
    {
        _buffFactory = buffFactory;
    }
    
    public BCardType HandledType => BCardType.MysticArtsTransformed;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        
        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.MysticArtsTransformed.CreateFullMoonBoundWhenEnemyBoundByMoonlight when target.BuffComponent.HasBuff((int)BuffVnums.BOUND_BY_MOONLIGHT):
                target.RemoveBuffAsync((int)BuffVnums.BOUND_BY_MOONLIGHT).ConfigureAwait(false).GetAwaiter().GetResult();
                target.AddBuffAsync(_buffFactory.CreateBuff((int)BuffVnums.BOUND_BY_THE_FULLMOONS_LIGHT, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}