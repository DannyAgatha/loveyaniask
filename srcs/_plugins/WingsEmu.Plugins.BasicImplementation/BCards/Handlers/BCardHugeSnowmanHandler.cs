// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHugeSnowmanHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.DamageInflict;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.DamageInflict.SnowStorm:
                {
                    IEnumerable<IBattleEntity> toDamage = sender.GetEnemiesInRange(sender, 100).Take(45);

                    foreach (IBattleEntity entity in toDamage)
                    {
                        if (!entity.IsAlive())
                        {
                            continue;
                        }

                        if (entity is IPlayerEntity playerEntity && playerEntity.SkillComponent.PyjamaFakeDeadActivated)
                        {
                            return;
                        }

                        int damage = (int)(entity.MaxHp * (50 * 0.01));

                        if (entity.Hp - damage <= 0)
                        {
                            entity.Hp = 1;
                            sender.BroadcastCleanSuPacket(entity, damage);
                            continue;
                        }

                        entity.Hp -= damage;

                        switch (entity)
                        {
                            case IPlayerEntity character:
                                character.LastDefence = DateTime.UtcNow;
                                character.Session.RefreshStat();
                                break;
                        }

                        sender.BroadcastCleanSuPacket(entity, damage);
                    }
                }


                break;
            case (byte)AdditionalTypes.DamageInflict.EarthQuake:
                {
                    IEnumerable<IBattleEntity> toDamage = sender.GetEnemiesInRange(sender, 100).Take(45);

                    foreach (IBattleEntity entity in toDamage)
                    {
                        if (!entity.IsAlive())
                        {
                            continue;
                        }

                        if (entity is IPlayerEntity playerEntity && playerEntity.SkillComponent.PyjamaFakeDeadActivated == false)
                        {
                            return;
                        }

                        int damage = (int)(entity.MaxHp * (50 * 0.01));

                        if (entity.Hp - damage <= 0)
                        {
                            entity.Hp = 1;
                            sender.BroadcastCleanSuPacket(entity, damage);
                            continue;
                        }

                        entity.Hp -= damage;

                        switch (entity)
                        {
                            case IPlayerEntity character:
                                character.LastDefence = DateTime.UtcNow;
                                character.Session.RefreshStat();
                                break;
                        }

                        sender.BroadcastCleanSuPacket(entity, damage);
                    }
                }
                break;
        }
    }
}