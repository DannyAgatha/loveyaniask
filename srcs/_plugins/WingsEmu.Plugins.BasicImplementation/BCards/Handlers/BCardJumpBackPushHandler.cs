// NosEmu
// 


using System;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardJumpBackPushHandler : IBCardEffectAsyncHandler
{
    private readonly IRandomGenerator _random;

    public BCardJumpBackPushHandler(IRandomGenerator random) => _random = random;

    public BCardType HandledType => BCardType.JumpBackPush;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstData;

        if (_random.RandomNumber() > firstData)
        {
            return;
        }

        int secondData = ctx.BCard.SecondData;
        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.JumpBackPush.JumpBackChance:
                short sX = target.PositionX;
                short sY = target.PositionY;
                short tX = sender.PositionX;
                short tY = sender.PositionY;

                int distance = sender.GetDistance(target);
                if (distance <= 0)
                {
                    distance = 1;
                }

                int d = distance + secondData;

                short maxX = (short)(sX - d * (sX - tX) / distance);
                short maxY = (short)(sY - d * (sY - tY) / distance);

                int dx = Math.Abs(maxX - tX);
                int sx = tX < maxX ? 1 : -1;
                int dy = -Math.Abs(maxY - tY);
                int sy = tY < maxY ? 1 : -1;
                int err = dx + dy;

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

                if (sender is IPlayerEntity playerEntity && playerEntity.MapInstance != null && playerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                {
                    playerEntity.MapX = tX;
                    playerEntity.MapY = tY;
                }

                sender.MapInstance?.Broadcast(sender.GeneratePushPacket(tX, tY, 2));
                sender.ChangePosition(new Position(tX, tY));
                break;
            case (byte)AdditionalTypes.JumpBackPush.PushBackChance:
            case (byte)AdditionalTypes.JumpBackPush.PushBackChancePvP:
                
                if (ctx.BCard.SubType == (byte)AdditionalTypes.JumpBackPush.PushBackChancePvP && !target.IsPlayer())
                {
                    return;
                }
                
                if (!target.IsAlive())
                {
                    return;
                }

                if (target is IMonsterEntity { CanBePushed: false })
                {
                    return;
                }

                (int resistance, _) = target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.ResistForcedMovement, target.Level);
                if (resistance != 0 && _random.RandomNumber() <= resistance)
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

                d = distance + secondData;

                maxX = (short)(tX + d * (sX - tX) / distance);
                maxY = (short)(tY + d * (sY - tY) / distance);

                dx = Math.Abs(maxX - tX);
                sx = tX < maxX ? 1 : -1;
                dy = -Math.Abs(maxY - tY);
                sy = tY < maxY ? 1 : -1;
                err = dx + dy;

                lastX = tX;
                lastY = tY;

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

                if (target is IPlayerEntity player && player.MapInstance != null && player.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                {
                    player.MapX = tX;
                    player.MapY = tY;
                }

                sender.MapInstance?.Broadcast(target.GeneratePushPacket(tX, tY, 2));
                target.ChangePosition(new Position(tX, tY));
                break;
        }
    }
}