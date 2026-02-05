// NosEmu
// 


using System;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialActionsHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _randomGenerator;

    public BCardSpecialActionsHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public BCardType HandledType => BCardType.SpecialActions;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity target = ctx.Target;
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        switch (subType)
        {
            case (byte)AdditionalTypes.SpecialActions.RunAway:

                if (target is IMonsterEntity monsterEntity)
                {
                    if (sender.Level < monsterEntity.Level)
                    {
                        return;
                    }

                    monsterEntity.BroadcastChatBubble("!!!", ChatMessageColorType.PlayerSay);
                    monsterEntity.IsRunningAway = true;
                }

                if (target is not INpcEntity npcEntity)
                {
                    return;
                }

                if (sender.Level < npcEntity.Level)
                {
                    return;
                }

                npcEntity.BroadcastChatBubble("!!!", ChatMessageColorType.PlayerSay);
                npcEntity.IsRunningAway = true;
                break;
            case (byte)AdditionalTypes.SpecialActions.Hide:
                {
                    if (sender is IMateEntity mate)
                    {
                        IPlayerEntity owner = mate.Owner;
                        if (!owner.Position.IsInRange(mate.Position, 2))
                        {
                            return;
                        }
                        owner.CharacterInvisible();
                        return;
                    }

                    if (target is not IPlayerEntity character)
                    {
                        return;
                    }

                    character.CharacterInvisible();
                }
                break;
            case (byte)AdditionalTypes.SpecialActions.FocusEnemies:
                if (!target.IsAlive())
                {
                    return;
                }

                switch (target)
                {
                    case INpcEntity { CanAttack: false }:
                        return;
                    case IMonsterEntity { CanBePushed: false }:
                        return;
                }

                (int resistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                if (resistance != 0 && _randomGenerator.RandomNumber() <= resistance)
                {
                    return;
                }

                short sX = sender.PositionX;
                short sY = sender.PositionY;
                short tX = target.PositionX;
                short tY = target.PositionY;

                int distance = sender.GetDistance(target);
                if (distance <= 0)
                {
                    distance = 1;
                }

                int d = firstData;

                short maxX = (short)(sX - d * (sX - tX) / distance);
                short maxY = (short)(sY - d * (sY - tY) / distance);

                int dx = Math.Abs(maxX - tX);
                int sx = tX < maxX ? 1 : -1;
                int dy = -Math.Abs(maxY - tY);
                int sy = tY < maxY ? 1 : -1;
                int err = dx + dy;

                bool isLineOfSight = true;

                while (true)
                {
                    if (sender.MapInstance.IsBlockedZone(tX, tY))
                    {
                        isLineOfSight = false;
                        break;
                    }

                    if (tX == maxX && tY == maxY)
                    {
                        break;
                    }

                    int e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        tX += (short)sx;
                    }

                    if (e2 > dx)
                    {
                        continue;
                    }

                    err += dx;
                    tY += (short)sy;
                }

                if (!isLineOfSight)
                {
                    return;
                }

                var newPosition = new Position(tX, tY);
                switch (target)
                {
                    case IPlayerEntity playerEntity:
                        if (playerEntity.IsSitting)
                        {
                            playerEntity.Session.RestAsync(force: true);
                        }

                        break;
                    case IMateEntity mate:
                        if (mate.IsSitting)
                        {
                            mate.Owner.Session.EmitEvent(new MateRestEvent
                            {
                                MateEntity = mate,
                                Rest = false,
                                Force = true
                            });
                        }
                        break;

                    case IMonsterEntity monster:
                        if (!monster.Targets.Contains(target))
                        {
                            target.MapInstance.AddEntityToTargets(monster, sender);
                            monster.BroadcastEffectInRange(EffectType.Targeted);
                        }
                        break;
                }

                sender.MapInstance?.Broadcast(target.GeneratePushPacket(maxX, maxY, secondData));
                target.BroadcastEffectInRange(EffectType.PushSmoke);
                target.ChangePosition(newPosition);

                break;
            case (byte)AdditionalTypes.SpecialActions.PushBack:
            {
                if (!target.IsAlive())
                {
                    return;
                }

                switch (target)
                {
                    case INpcEntity { CanAttack: false }:
                        return;
                    case IMonsterEntity { CanBePushed: false }:
                        return;
                }

                (int pushResistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                if (pushResistance != 0 && _randomGenerator.RandomNumber() <= pushResistance)
                {
                    return;
                }

                sX = sender.PositionX;
                sY = sender.PositionY;
                tX = target.PositionX;
                tY = target.PositionY;


                distance = sender.GetDistance(target);
                if (distance <= 0)
                {
                    distance = 1;
                }

                d = distance + firstData;

                maxX = (short)(sX - d * (sX - tX) / distance);
                maxY = (short)(sY - d * (sY - tY) / distance);

                dx = Math.Abs(maxX - tX);
                sx = tX < maxX ? 1 : -1;
                dy = -Math.Abs(maxY - tY);
                sy = tY < maxY ? 1 : -1;
                err = dx + dy;

                short lastX = tX;
                short lastY = tY;

                while (true)
                {
                    if (sender.MapInstance.IsBlockedZone(tX, tY))
                    {
                        tX = lastX;
                        tY = lastY;
                        break;
                    }

                    lastX = tX;
                    lastY = tY;

                    if (tX == maxX && tY == maxY)
                    {
                        break;
                    }

                    int e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        tX += (short)sx;
                    }

                    if (e2 > dx)
                    {
                        continue;
                    }

                    err += dx;
                    tY += (short)sy;
                }

                newPosition = new Position(tX, tY);
                switch (target)
                {
                    case IPlayerEntity playerEntity:
                        if (playerEntity.IsSitting)
                        {
                            playerEntity.Session.RestAsync(force: true).ConfigureAwait(false).GetAwaiter().GetResult();
                        }

                        break;
                    case IMateEntity mate:

                        if (mate.IsSitting)
                        {
                            mate.Owner.Session.EmitEvent(new MateRestEvent
                            {
                                MateEntity = mate,
                                Rest = false,
                                Force = true
                            });
                        }

                        break;
                }
                
                sender.MapInstance?.Broadcast(target.GeneratePushPacket(tX, tY, secondData));
                target.ChangePosition(newPosition);
                target.BroadcastEffectInRange(EffectType.PushSmoke);

                break;
            }
            case (byte)AdditionalTypes.SpecialActions.Charge:
                if (target == null)
                {
                    return;
                }

                if (!target.IsAlive())
                {
                    return;
                }

                newPosition = target.Position;
                sender.MapInstance?.Broadcast(sender.GenerateDashGuriPacket(newPosition.X, newPosition.Y, secondData));
                sender.ChangePosition(newPosition);

                break;
            case (byte)AdditionalTypes.SpecialActions.ChargeNegated:

                if (!target.IsAlive())
                {
                    return;
                }

                sX = target.PositionX;
                sY = target.PositionY;
                tX = sender.PositionX;
                tY = sender.PositionY;

                distance = sender.GetDistance(target);
                if (distance <= 0)
                {
                    distance = 1;
                }

                d = firstData;

                maxX = (short)(sX - d * (sX - tX) / distance);
                maxY = (short)(sY - d * (sY - tY) / distance);

                dx = Math.Abs(maxX - tX);
                sx = tX < maxX ? 1 : -1;
                dy = -Math.Abs(maxY - tY);
                sy = tY < maxY ? 1 : -1;
                err = dx + dy;

                isLineOfSight = true;

                while (true)
                {
                    if (sender.MapInstance.IsBlockedZone(tX, tY))
                    {
                        isLineOfSight = false;
                        break;
                    }

                    if (tX == maxX && tY == maxY)
                    {
                        break;
                    }

                    int e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        tX += (short)sx;
                    }

                    if (e2 > dx)
                    {
                        continue;
                    }

                    err += dx;
                    tY += (short)sy;
                }

                if (!isLineOfSight)
                {
                    return;
                }

                newPosition = new Position(tX, tY);
                sender.MapInstance?.Broadcast(sender.GenerateDashGuriPacket(tX, tY, secondData));
                sender.ChangePosition(newPosition);

                break;
        }
    }
}