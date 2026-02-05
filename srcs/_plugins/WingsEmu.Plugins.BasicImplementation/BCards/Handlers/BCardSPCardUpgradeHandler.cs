using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSPCardUpgradeHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    public BCardSPCardUpgradeHandler(IBuffFactory buffFactory)
    {
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.SPCardUpgrade;

    public async void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;
        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.SPCardUpgrade.ApplyBuffToEntity:
            {
                if (target is null)
                {
                    return;
                }
                    
                Buff buff = _buffFactory.CreateBuff(target.IsPlayer() ? secondData : firstData, sender);
                await target.AddBuffAsync(buff);
            }
                break;
        }
    }
}