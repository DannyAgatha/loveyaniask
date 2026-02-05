using System;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardStealBuffHandler : IBCardEffectAsyncHandler
    {
        private readonly IRandomGenerator _randomGenerator;
        private readonly IScheduler _scheduler;
        private readonly IMonsterEntityFactory _monsterEntityFactory;
        private readonly IAsyncEventPipeline _eventPipeline;
        
        public BCardStealBuffHandler(IRandomGenerator randomGenerator, IScheduler scheduler, IMonsterEntityFactory monsterEntityFactory, IAsyncEventPipeline eventPipeline)
        {
            _randomGenerator = randomGenerator;
            _scheduler = scheduler;
            _monsterEntityFactory = monsterEntityFactory;
            _eventPipeline = eventPipeline;
        }

        public BCardType HandledType => BCardType.StealBuff;

        public void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity sender = ctx.Sender;
            IBattleEntity target = ctx.Target;
            SkillInfo skill = ctx.Skill;
            SuPacketHitMode hitMode = ctx.HitMode;
            int damageDealt = ctx.DamageDealt;
            int firstData = ctx.BCard.FirstDataValue(sender.Level);
            int secondData =  ctx.BCard.SecondDataValue(sender.Level);
            byte subType = ctx.BCard.SubType;

            switch (subType)
            {
                case (byte)AdditionalTypes.StealBuff.RemoveMorph:

                    if (_randomGenerator.RandomNumber() > firstData)
                    {
                        return;
                    }
                    
                    if (target is not IPlayerEntity player)
                    {
                        return;
                    }
                    
                    bool isSpecialistMorphed = player is { UseSp: true };

                    if (!isSpecialistMorphed)
                    {
                        return;
                    }
                    
                    player.BlockAllAttack = true;
                    player.WasMorphedPreviously = true;
                    player.Session.RefreshStat();
                    
                    player.Session.EmitEvent(new SpUntransformEvent
                    {
                        Force = true
                    });
                    
                    _scheduler.Schedule(TimeSpan.FromSeconds(secondData), () =>
                    {
                        player.BlockAllAttack = false;
                        player.Session.RefreshStat();
                    });
                    break;
                
                case (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon:
                    
                    if (skill?.CastId is 0 || damageDealt is 0)
                    {
                        return;
                    }
                    
                    if (!sender.IsSucceededChance(firstData))
                    {
                        return;
                    }
                    
                    if (sender is not IPlayerEntity playerEntity || playerEntity.SkillComponent.OnyxMonster != null)
                    {
                        return;
                    }
                    
                    if (playerEntity.Session.CurrentMapInstance.MapVnum == (int)MapIds.SNOWMAN_BOSS_ROOM)
                    {
                        return;
                    }
                    
                    short x = playerEntity.PositionX;
                    short y = playerEntity.PositionY;

                    x += (short)_randomGenerator.RandomNumber(-3, 3);
                    y += (short)_randomGenerator.RandomNumber(-3, 3);
                    
                    if (playerEntity.MapInstance.IsBlockedZone(x, y))
                    {
                        x = playerEntity.PositionX;
                        y = playerEntity.PositionY;
                    }
                    
                    playerEntity.SkillComponent.OnyxMonster = _monsterEntityFactory.CreateMonster((int)MonsterVnum.ONYX_MONSTER, playerEntity.MapInstance, new MonsterEntityBuilder
                    {
                        IsRespawningOnDeath = false,
                        IsWalkingAround = false
                    });

                    playerEntity.SkillComponent.OnyxMonster.EmitEventAsync(new MapJoinMonsterEntityEvent(playerEntity.SkillComponent.OnyxMonster, x, y));
                    playerEntity.MapInstance.Broadcast(playerEntity.GenerateOnyxGuriPacket(x, y));
                    
                    int onyxDamage = damageDealt / 2;
                    _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                    {
                        Damaged = target,
                        Damager = playerEntity.SkillComponent.OnyxMonster,
                        Damage = onyxDamage,
                        CanKill = false,
                        SkillInfo = skill
                    });

                    playerEntity.SkillComponent.OnyxMonster.BroadcastSuPacket(target, skill, onyxDamage, hitMode);
                    break;
            }
        }
    }
}