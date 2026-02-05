using System;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBeriosHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;

    public BCardBeriosHandler(IAsyncEventPipeline eventPipeline, GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory)
    {
        _eventPipeline = eventPipeline;
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.LordBerios;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.LordBerios.CauseDamage:
            {
                {
                    IEnumerable<IBattleEntity> toDamage = sender.GetEnemiesInRange(sender, (byte)firstDataValue).Take(50);

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
                                    character.Session.RestAsync(force: true);
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
                }
                break;
            }
            
            case (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence:
            {
                if (sender is not IPlayerEntity character)
                {
                    return;
                }

                character.CharacterInvisible();
                character.Session.Broadcast(character.Session.GenerateInvisible());
            }
                break;
        }
    }
}