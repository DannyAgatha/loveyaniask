using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMateSummonHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;

    public BCardMateSummonHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
    }

    public BCardType HandledType => BCardType.SummonAndRecoverHP;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        int firstData = bCard.FirstDataValue(sender.Level);
        int secondData = bCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.SummonAndRecoverHP.ChanceSummon:
            {
                if (_randomGenerator.RandomNumber() > firstData)
                {
                    return;
                }

                if (sender is not IMateEntity mateEntity)
                {
                    return;
                }

                IPlayerEntity owner = mateEntity.Owner;

                if (owner == null)
                {
                    return;
                }

                if (owner.MapInstance.Id != mateEntity.MapInstance.Id)
                {
                    return;
                }

                int amount = _randomGenerator.RandomNumber(1, 4);

                var monsters = new List<ToSummon>();
                Position position = mateEntity.Position;

                for (int i = 0; i < amount; i++)
                {
                    int x = position.X + _randomGenerator.RandomNumber(-2, 2);
                    int y = position.Y + _randomGenerator.RandomNumber(-2, 2);

                    if (mateEntity.MapInstance.IsBlockedZone(x, y))
                    {
                        x = position.X;
                        y = position.Y;
                    }

                    var toSummon = new ToSummon
                    {
                        VNum = (short)secondData,
                        SpawnCell = new Position((short)x, (short)y),
                        IsMoving = true,
                        IsHostile = true
                    };

                    monsters.Add(toSummon);
                }

                _asyncEvent.ProcessEventAsync(new MonsterSummonEvent(owner.MapInstance, monsters, owner)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
            case (byte)AdditionalTypes.SummonAndRecoverHP.RestoreHP:
            {
                if (sender is not IMonsterEntity monsterEntity)
                {
                    return;
                }

                if (!monsterEntity.SummonerId.HasValue)
                {
                    return;
                }

                IMateEntity mate = monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)?.MateComponent.GetTeamMember(x => x.MateType == MateType.Pet);
                if (mate == null)
                {
                    return;
                }

                if (monsterEntity.MapInstance.Id != mate.MapInstance.Id)
                {
                    return;
                }

                if (mate.Owner.IsOnVehicle)
                {
                    return;
                }

                int toHeal = (int)(mate.MaxHp * (firstData * 0.01));
                mate.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = mate,
                    HpHeal = toHeal
                });

                monsterEntity.BroadcastEffectTarget(mate, EffectType.MateHealByMonster);
            }
                break;

            case (byte)AdditionalTypes.SummonAndRecoverHP.HolyEnergyBarRefilled:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                player.LastEnergyRefill = DateTime.UtcNow;
                player.UpdateEnergyBar(firstData).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;

            case (byte)AdditionalTypes.SummonAndRecoverHP.HolyEnergyBarEmptied:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                player.LastEnergyRefill = DateTime.UtcNow;
                player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;

            case (byte)AdditionalTypes.SummonAndRecoverHP.CasterMaxHPRegenNoSelf:
            {
                int heal = (int)(sender.MaxHp * firstData * 0.01);
                
                if (!target.Equals(sender))
                {
                    target.EmitEvent(new BattleEntityHealEvent
                    {
                        Entity = target,
                        HpHeal = heal
                    });
                }
            }
                break;

        }
    }
}