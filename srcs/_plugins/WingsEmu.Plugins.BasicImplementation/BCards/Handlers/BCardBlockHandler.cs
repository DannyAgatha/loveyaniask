using PhoenixLib.Events;
using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBlockHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardBlockHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.Block;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        int damage = ctx.DamageDealt;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.Block.DecreaseFinalCriticalDamagePerHit:
            {
                sender.BCardDataComponent.CriticalDamageDecreased = 0;
            }
                break;
        }
    }
}