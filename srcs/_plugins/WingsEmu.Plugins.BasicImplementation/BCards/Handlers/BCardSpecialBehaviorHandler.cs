// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialBehaviorHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IMonsterEntityFactory _monsterEntityFactory;

    public BCardSpecialBehaviorHandler(IBuffFactory buffFactory, IAsyncEventPipeline asyncEventPipeline, IMonsterEntityFactory monsterEntityFactory)
    {
        _buffFactory = buffFactory;
        _asyncEventPipeline = asyncEventPipeline;
        _monsterEntityFactory = monsterEntityFactory;
    }

    public BCardType HandledType => BCardType.SpecialBehaviour;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (subType)
        {
            case (byte)AdditionalTypes.SpecialBehaviour.InflictOnTeam:
                IEnumerable<IBattleEntity> allies = target.Position.GetAlliesInRange(target, (byte)firstData);
                foreach (IBattleEntity entity in allies)
                {
                    if (entity.BuffComponent.HasBuff(secondData))
                    {
                        continue;
                    }

                    if (!ShouldGiveDebuff(target, entity))
                    {
                        continue;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    entity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                break;
            
            case (byte)AdditionalTypes.SpecialBehaviour.InflictOnEnemies:
                IEnumerable<IBattleEntity> enemies = target.Position.GetEnemiesInRange(target, (byte)firstData);
                foreach (IBattleEntity entity in enemies)
                {
                    if (entity.BuffComponent.HasBuff(secondData))
                    {
                        continue;
                    }

                    if (!ShouldGiveDebuff(target, entity))
                    {
                        continue;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    entity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                break;
            case (byte)AdditionalTypes.SpecialBehaviour.TeleportRandom:

                Position newPosition = sender.MapInstance.GetRandomPosition();
                sender.TeleportOnMap(newPosition.X, newPosition.Y);

                break;
            case (byte)AdditionalTypes.SpecialBehaviour.JumpToEveryObject:

                if (sender is not IMonsterEntity monsterEntity)
                {
                    return;
                }

                if (monsterEntity.Target == null)
                {
                    return;
                }

                IBattleEntity monsterTarget = monsterEntity.Target;
                IEnumerable<IMonsterEntity> monsters = monsterEntity.MapInstance.GetAliveMonstersInRange(monsterEntity.Position, (byte)firstData);
                foreach (IMonsterEntity monster in monsters)
                {
                    if (monster.Id == monsterEntity.Id)
                    {
                        continue;
                    }

                    monster.MapInstance.AddEntityToTargets(monster, monsterTarget);
                    if (monsterTarget is not IPlayerEntity playerEntity)
                    {
                        continue;
                    }

                    playerEntity.Session.SendEffectEntity(monster, EffectType.TargetedByOthers);
                }

                break;
            case (byte)AdditionalTypes.SpecialBehaviour.TransformInto:
                if (sender is not IMonsterEntity bossMonsterEntity)
                {
                    return;
                }

                IEnumerable<IMonsterEntity> monstersToTransform = bossMonsterEntity.MapInstance.GetAliveMonsters(x => x.MonsterVNum == firstData);

                foreach (IMonsterEntity monster in monstersToTransform)
                {
                    Position position = monster.Position;
                    _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster));

                    IMonsterEntity newMonster = _monsterEntityFactory.CreateMonster(secondData, bossMonsterEntity.MapInstance, new MonsterEntityBuilder
                    {
                        Direction = 2,
                        IsRespawningOnDeath = false,
                        PositionX = position.X,
                        PositionY = position.Y,
                        IsHostile = true,
                        IsWalkingAround = true
                    });
                    
                    newMonster.EmitEvent(new MapJoinMonsterEntityEvent(newMonster, position.X, position.Y, true));
                }
                
                break;
        }
    }
    
    private bool ShouldGiveDebuff(IBattleEntity debuffer, IBattleEntity receiver)
    {
        bool give = debuffer switch
        {
            IPlayerEntity player when receiver is IPlayerEntity receiverPlayer => player.IsInGroupOf(receiverPlayer),
            IPlayerEntity player when receiver is IMateEntity mateEntity => player.IsInGroupOf(mateEntity.Owner),
            IMateEntity mateEntity when receiver is IPlayerEntity receiverPlayer => mateEntity.Owner.Id == receiverPlayer.Id,
            IMateEntity mateEntity when receiver is IMateEntity mate => mateEntity.Owner.Id == mate.Owner.Id,
            INpcEntity => false,
            _ => true
        };

        return give;
    }
}