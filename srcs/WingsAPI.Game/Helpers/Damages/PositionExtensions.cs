using System;
using System.Collections.Generic;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;

namespace WingsEmu.Game.Helpers.Damages;

public static class PositionExtensions
{
    private static readonly double SQRT_2 = Math.Sqrt(2);

    public static IReadOnlyList<IBattleEntity> GetAlliesInRange(this Position position, IBattleEntity caster, short range)
    {
        var allies = new List<IBattleEntity>();

        IReadOnlyList<IBattleEntity> entities = caster.MapInstance.GetClosestBattleEntitiesInRange(position, range);

        foreach (IBattleEntity entity in entities)
        {
            if (entity is IPlayerEntity { IsSeal: true })
            {
                continue;
            }

            if (caster.IsAllyWith(entity) && entity.GetMonsterRaceType() != MonsterRaceType.Fixed)
            {
                allies.Add(entity);
            }
        }

        return allies;
    }

    public static IReadOnlyList<IBattleEntity> GetEnemiesInRange(this Position position, IBattleEntity caster, short range)
    {
        var enemies = new List<IBattleEntity>();
        IEnumerable<IBattleEntity> entities = caster.MapInstance.GetClosestBattleEntitiesInRange(position, range);

        foreach (IBattleEntity entity in entities)
        {
            if (caster.IsEnemyWith(entity) && entity.GetMonsterRaceType() != MonsterRaceType.Fixed)
            {
                enemies.Add(entity);
            }
        }

        return enemies;
    }

    private static double Octile(int x, int y)
    {
        int min = Math.Min(x, y);
        int max = Math.Max(x, y);
        return min * SQRT_2 + max - min;
    }

    public static bool IsInAoeZone(this Position start, Position position, int range)
    {
        int dx = Math.Abs(start.X - position.X);
        int dy = Math.Abs(start.Y - position.Y);
        bool s = dx <= range && dy <= range;
        return range switch
        {
            < 2 => s,
            < 5 => s && dx + dy < range + range,
            _ => s && dx + dy <= range + range / 2
        };
    }

    public static bool IsInAoeZone(this Position start, IBattleEntity to, int range) => IsInAoeZone(start, to.Position, range);
    public static bool IsInAoeZone(this IBattleEntity start, IBattleEntity to, int range) => IsInAoeZone(start.Position, to.Position, range);
    public static bool IsInAoeZone(this IBattleEntityDump start, IBattleEntityDump to, int range) => IsInAoeZone(start.Position, to.Position, range);

    public static bool IsInPvpZone(this IBattleEntity target) => target?.MapInstance != null && !target.MapInstance.PvpZone(target.PositionX, target.PositionY);
    public static bool IsInMateDollZone(this IBattleEntity entity) => entity.MapInstance != null && !entity.MapInstance.MateDollZone(entity.PositionX, entity.PositionY);

    public static bool IsInRange(this IBattleEntityDump from, IBattleEntityDump to, int range) => IsInRange(from.Position, to.Position, range);

    public static bool IsInRange(this Position src, short x, short y, int range) => GetDistance(src, x, y) <= range;
    public static bool IsInRange(this Position src, Position pos, int range) => GetDistance(src, pos) <= range;

    public static int GetDistance(this IBattleEntityDump from, IBattleEntityDump to) => GetDistance(from.Position, to.Position);
    public static int GetDistance(this Position src, Position dest) => GetDistance(src, dest.X, dest.Y);
    public static double GetDoubleDistance(this Position src, Position dest) => GetDoubleDistance(src, dest.X, dest.Y);
    public static int GetDistance(this Position src, int x, int y) => (int)Octile(Math.Abs(src.X - x), Math.Abs(src.Y - y));
    public static double GetDoubleDistance(this Position src, int x, int y) => Math.Sqrt(Math.Pow(src.X - x, 2) + Math.Pow(src.Y - y, 2));
    public static int GetDistance(this IBattleEntity src, IBattleEntity to) => GetDistance(src.Position, to.Position);

    public static Position NewMinilandMapCell(this IBattleEntity entity, IRandomGenerator randomGenerator)
    {
        short newX = (short)randomGenerator.RandomNumber(5, 15);
        short newY = (short)randomGenerator.RandomNumber(3, 14);
        return new Position(newX, newY);
    }

    public static bool IsInLineX(this IBattleEntity entity, short x, short width) => Math.Abs(entity.Position.X - x) <= width;
    public static bool IsInLineY(this IBattleEntity entity, short y, short height) => Math.Abs(entity.Position.Y - y) <= height;
    public static bool IsMonsterAggroDisabled(this IBattleEntity entity) => entity.MapInstance != null && !entity.MapInstance.IsMonsterAggroDisabled(entity.PositionX, entity.PositionY);
    public static bool IsMonsterAggroDisabled(this IBattleEntity entity, short x, short y) => entity.MapInstance != null && !entity.MapInstance.IsMonsterAggroDisabled(x, y);

    public static void ChangePosition(this IBattleEntity battleEntity, Position newPosition)
    {
        battleEntity.Position = newPosition;

        if (battleEntity.MapInstance == null)
        {
            return;
        }

        switch (battleEntity)
        {
            case IPlayerEntity playerEntity:

                if (playerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && playerEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
                {
                    playerEntity.MapX = newPosition.X;
                    playerEntity.MapY = newPosition.Y;
                }

                break;
            case IMateEntity mateEntity:

                if (mateEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && mateEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
                {
                    mateEntity.MapX = newPosition.X;
                    mateEntity.MapX = newPosition.Y;
                }

                if (mateEntity.MapInstance is { MapInstanceType: MapInstanceType.Miniland } && mateEntity.MapInstance?.Id == mateEntity.Owner?.Miniland?.Id)
                {
                    mateEntity.MinilandX = newPosition.X;
                    mateEntity.MinilandX = newPosition.Y;
                }

                break;
        }
    }
}