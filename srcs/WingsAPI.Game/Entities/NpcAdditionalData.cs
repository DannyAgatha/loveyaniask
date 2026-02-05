using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public class NpcAdditionalData : INpcAdditionalData
{
    public bool IsTimeSpaceMate { get; init; }
    public bool IsProtected { get; init; }
    public IPlayerEntity MinilandOwner { get; init; }
    public bool NpcShouldRespawn { get; init; }
    public bool CanMove { get; init; }
    public bool CanAttack { get; init; }
    public byte NpcDirection { get; init; }
    public bool IsHostile { get; init; }
    public float? HpMultiplier { get; init; }
    public float? MpMultiplier { get; init; }
    public byte? CustomLevel { get; init; }
    public FactionType FactionType { get; init; }
    public long? TimeSpaceOwnerId { get; init; }
    public TimeSpaceFileConfiguration TimeSpaceInfo { get; init; }
    public RainBowFlag RainbowFlag { get; init; }
}