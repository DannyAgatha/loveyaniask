using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardCountHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;

    public BCardCountHandler(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

    public BCardType HandledType => BCardType.Count;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        if (sender is not IMonsterEntity monsterEntity)
        {
            return;
        }

        int firstData = bCard.FirstDataValue(monsterEntity.Level);
        int secondData = bCard.SecondDataValue(monsterEntity.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.Count.Summon:

                if (monsterEntity.Mp > 0)
                {
                    return;
                }

                var summons = new List<ToSummon>();

                for (int i = 0; i < firstData; i++)
                {
                    summons.Add(new ToSummon
                    {
                        VNum = (short)secondData,
                        SpawnCell = monsterEntity.Position,
                        IsMoving = true,
                        IsHostile = true
                    });
                }

                _eventPipeline.ProcessEventAsync(new
                    MonsterSummonEvent(sender.MapInstance, summons, showEffect: true, summoner: monsterEntity)).ConfigureAwait(false).GetAwaiter().GetResult();

                break;
        }
    }
}