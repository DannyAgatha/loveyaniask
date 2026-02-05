// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Monster;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game;

public class ToSummon
{
    public int VNum { get; init; }
    public Position? SpawnCell { get; init; }
    public IBattleEntity Target { get; init; }
    public bool IsMoving { get; init; }
    public byte SummonChance { get; init; } = 100;
    public bool RemoveTick { get; init; }
    public bool IsTarget { get; init; }
    public bool IsBonusOrProtected { get; init; }
    public bool IsHostile { get; init; }
    public bool IsBossOrMate { get; init; }
    public bool IsVesselMonster { get; init; }
    public bool IsVesselChristmasMonster { get; init; }
    public bool IsTotem { get; init; }
    public short? EffectId {  get; init; }
    public bool IsSparringMonster { get; init; }
    public SummonType? SummonType { get; init; }
    public IMonsterEntity MonsterEntity { get; init; }
    public IReadOnlyCollection<(string key, IAsyncEvent asyncEvent, bool removeOnUse)> TriggerEvents { get; init; }
    public bool IsMateTrainer { get; init; }
    public short SetHitChance { get; init; }
    public Position? GoToBossPosition { get; init; }
    public bool IsInstantBattle { get; init; }
    public byte Direction { get; init; } = 2;
    public byte? Level { get; init; }
    public float? HpMultiplier { get; init; }
    public float? MpMultiplier { get; init; }
    public FactionType? FactionType { get; init; }
    public Guid? AtAroundMobId { get; init; }
    public byte? AtAroundMobRange { get; init; }
    public ConcurrentDictionary<byte, Waypoint> Waypoints { get; init; }
    public bool IgnoreSkillRange { get; init; }
    public bool IncreaseTick { get; init; }
}