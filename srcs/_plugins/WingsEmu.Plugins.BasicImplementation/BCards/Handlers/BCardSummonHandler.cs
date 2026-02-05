// NosEmu
// 


using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSummonHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly INpcMonsterManager _manager;
    private readonly IRandomGenerator _randomGenerator;

    public BCardSummonHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline, INpcMonsterManager manager)
    {
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
        _manager = manager;
    }

    public BCardType HandledType => BCardType.Summons;

    public void Execute(IBCardEffectContext ctx)
    {
        if (ctx.Sender == null)
        {
            return;
        }

        if (ctx.Target == null)
        {
            return;
        }

        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;

        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;
        int procChance = ctx.BCard.ProcChance;

        var summons = new List<ToSummon>();

        Position entityPosition = sender.Position;

        switch (subType)
        {
            case (byte)AdditionalTypes.Summons.Summons:
                
                if (sender.MapInstance.MapId == 2581 && secondData == 416)
                {
                    return;
                }
                
                if (secondData == 974 && sender.BuffComponent.HasBuff((int)BuffVnums.OPPORTUNITY_TO_ATTACK))
                {
                    firstData += 2;
                    sender.RemoveBuffAsync((int)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                
                for (int i = 0; i < firstData; i++)
                {
                    IMonsterData monsterToSummon = _manager.GetNpc(secondData);
                    if (monsterToSummon == null)
                    {
                        continue;
                    }

                    short x;
                    short y;

                    if (sender is IMateEntity mateEntity)
                    {
                        IBattleEntity target = mateEntity.MapInstance.GetBattleEntity(mateEntity.TargetVisualType, mateEntity.TargetId);
                        x = target.Position.X;
                        y = target.Position.Y;
                    }
                    else
                    {
                        x = entityPosition.X;
                        y = entityPosition.Y;
                    }

                    x += (short)_randomGenerator.RandomNumber(-3, 3);
                    y += (short)_randomGenerator.RandomNumber(-3, 3);

                    if (sender.MapInstance.IsBlockedZone(x, y))
                    {
                        x = entityPosition.X;
                        y = entityPosition.Y;
                    }

                    var position = new Position(x, y);
                    summons.Add(new ToSummon
                    {
                        VNum = monsterToSummon.MonsterVNum,
                        SpawnCell = position,
                        IsMoving = monsterToSummon.CanWalk,
                        IsHostile = true
                    });
                }

                IBattleEntity summoner = sender.IsMate() ? (sender as IMateEntity)?.Owner : sender;
                _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons, summoner, showEffect: true)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.Summons.SummonningChance:

                short posX = sender.Position.X;
                short posY = sender.Position.Y;

                posX += (short)_randomGenerator.RandomNumber(-3, 3);
                posY += (short)_randomGenerator.RandomNumber(-3, 3);

                if (sender.MapInstance.IsBlockedZone(posX, posY))
                {
                    posX = entityPosition.X;
                    posY = entityPosition.Y;
                }

                var newPosition = new Position(posX, posY);

                summons.Add(new ToSummon
                {
                    VNum = (short)secondData,
                    SpawnCell = newPosition,
                    IsMoving = true,
                    IsHostile = true,
                    SummonChance = (byte)Math.Abs(firstData)
                });
                _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons, showEffect: true)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.Summons.SummonTrainingDummy:
                {
                    summons.Add(new ToSummon
                    {
                        VNum = (short)secondData,
                        SpawnCell = entityPosition,
                        IsMoving = true,
                        IsHostile = true,
                        IsMateTrainer = true,
                        SummonChance = 100
                    });
                    _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons, ctx.Sender, showEffect: true)).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
                }
            case (byte)AdditionalTypes.Summons.SummonUponDeathChance:
                summons.Add(new ToSummon
                {
                    VNum = (short)secondData,
                    SpawnCell = new Position(sender.PositionX, sender.PositionY),
                    IsMoving = true,
                    IsHostile = true,
                    SummonChance = (byte)Math.Abs(firstData)
                });
                _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.Summons.SummonUponDeath:
                for (short i = 0; i < firstData; i++)
                {
                    short senderPositionX = sender.Position.X;
                    short senderPositionY = sender.Position.Y;

                    senderPositionX += (short)_randomGenerator.RandomNumber(-2, 2);
                    senderPositionY += (short)_randomGenerator.RandomNumber(-2, 2);

                    if (sender.MapInstance.IsBlockedZone(senderPositionX, senderPositionY))
                    {
                        senderPositionX = entityPosition.X;
                        senderPositionY = entityPosition.Y;
                    }

                    summons.Add(new ToSummon
                    {
                        VNum = (short)secondData,
                        SpawnCell = new Position(senderPositionX, senderPositionY),
                        IsMoving = true,
                        IsHostile = true
                    });
                }

                _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}