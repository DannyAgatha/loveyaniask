using System;
using System.Collections.Generic;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public enum SummonType
{
    NORMAL,
    FROM_SKILL,
    MONSTER_WAVE,
    NPC_WAVE
}

public class MonsterEntityBuilder
{
    public bool IsBonus { get; init; }
    public bool IsBoss { get; init; }
    public bool IsHostile { get; init; }
    
    public bool IsTotem { get; init; }

    public short? EffectId { get; init; }
    public bool IsMateTrainer { get; init; }
    public bool IsRespawningOnDeath { get; init; }
    public bool IsTarget { get; init; }
    public bool IsVesselMonster { get; init; }
    public bool IsVesselChristmasMonster { get; init; }
    
    public bool IsSparringMonster { get; init; }
    public bool IsWalkingAround { get; init; }
    public short PositionX { get; init; }
    public short PositionY { get; init; }
    public byte Direction { get; init; } = 2;
    public short SetHitChance { get; init; }
    public Position? GoToBossPosition { get; init; }
    public bool IsInstantBattle { get; init; }
    public IEnumerable<DropChance> RaidDrop { get; init; }
    public byte? Level { get; init; }
    
    public byte? HeroLevel { get; init; }
    public SummonType? SummonType { get; init; }
    public long? SummonerId { get; init; }
    public VisualType? SummonerType { get; init; }
    public FactionType? FactionType { get; init; }

    public float? HpMultiplier { get; init; }
    public float? MpMultiplier { get; init; }

    public Guid? GeneratedGuid { get; init; }
    public bool IgnoreSkillRange { get; init; }
}

public interface IMonsterEntityFactory
{
    /// <summary>
    ///     Creates a default respawnable, walking around monster
    /// </summary>
    /// <param name="id"></param>
    /// <param name="monsterDto"></param>
    /// <param name="mapInstance"></param>
    /// <returns></returns>
    IMonsterEntity CreateMapMonster(MapMonsterDTO monsterDto, IMapInstance mapInstance);

    IMonsterEntity CreateMonster(int monsterVNum, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null);

    IMonsterEntity CreateMonster(int? id, int monsterVNum, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null);
    IMonsterEntity CreateMonster(IMonsterData monsterData, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null);
    IMonsterEntity CreateMonster(int? entityId, IMonsterData monsterData, IMapInstance mapInstance, MonsterEntityBuilder monsterAdditionalData = null);
}