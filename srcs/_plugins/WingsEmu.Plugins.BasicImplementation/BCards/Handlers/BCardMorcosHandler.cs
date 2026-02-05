using System;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.SnackFood.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMorcosHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;

    public BCardMorcosHandler(IAsyncEventPipeline eventPipeline, GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory,
        INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory)
    {
        _eventPipeline = eventPipeline;
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffFactory = buffFactory;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
    }

    public BCardType HandledType => BCardType.LordMorcos;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.LordMorcos.InflictDamageAfter:

                IEnumerable<IBattleEntity> toDamage = sender.MapInstance.GetNonMonsterBattleEntities(x => !sender.Position.IsInAoeZone(x.Position, firstDataValue)).Take(50);

                foreach (IBattleEntity entity in toDamage)
                {
                    if (!entity.IsAlive())
                    {
                        continue;
                    }

                    int damage = (int)(entity.MaxHp * (secondDataValue * 0.01));

                    if (sender.ShouldSaveDefender(entity, damage).ConfigureAwait(false).GetAwaiter().GetResult())
                    {
                        continue;
                    }

                    if (entity.Hp - damage <= 0)
                    {
                        entity.Hp = 0;
                        entity.EmitEvent(new GenerateEntityDeathEvent
                        {
                            Entity = entity,
                            Attacker = sender
                        });

                        sender.BroadcastCleanSuPacket(entity, damage);
                        continue;
                    }

                    entity.Hp -= damage;


                    switch (entity)
                    {
                        case IPlayerEntity character:
                            character.LastDefence = DateTime.UtcNow;
                            character.Session.RefreshStat();

                            if (character.IsSitting)
                            {
                                character.Session.RestAsync(force: true).ConfigureAwait(false).GetAwaiter().GetResult();
                            }

                            break;
                        case IMateEntity mate:
                            mate.LastDefence = DateTime.UtcNow;
                            mate.Owner.Session.SendMateLife(mate);

                            if (mate.IsSitting)
                            {
                                mate.Owner.Session.EmitEvent(new MateRestEvent
                                {
                                    MateEntity = mate,
                                    Force = true
                                });
                            }

                            break;
                    }

                    sender.BroadcastCleanSuPacket(entity, damage);
                }

                break;
            
            case (byte)AdditionalTypes.LordMorcos.GainAdditionalHpSunWolf:
            {
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }
                
                IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

                if (sunWolf is null)
                {
                    return;
                }

                int hpIncreased = (int)(sunWolf.MaxHp * firstDataValue * 0.01);

                playerEntity.Session.EmitEvent(new AddAdditionalHpMpEvent
                {
                    Hp = hpIncreased,
                    Mp = 0,
                    MaxHpPercentage = secondDataValue,
                    MaxMpPercentage = 0
                });
            }
                break;
            case (byte)AdditionalTypes.LordMorcos.CasterAndSunWolfChanceReceive:
            {
                if (!sender.IsSucceededChance(firstDataValue))
                {
                    return;
                }
                
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }
                
                Buff buff = _buffFactory.CreateBuff(secondDataValue, playerEntity, BuffFlag.NORMAL);
                playerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                
                IMateEntity sunWolf = playerEntity.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

                if (sunWolf is not null)
                {
                    sunWolf.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

            }
                break;
        }
    }
}