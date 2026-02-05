using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Entities;

public interface INpcAdditionalData
{
    bool IsTimeSpaceMate { get; }
    bool IsProtected { get; }
    IPlayerEntity MinilandOwner { get; }
    bool NpcShouldRespawn { get; }
    bool CanMove { get; }
    public bool CanAttack { get; }
    public byte NpcDirection { get; }
    public bool IsHostile { get; }
    public float? HpMultiplier { get; }
    public float? MpMultiplier { get; }
    public byte? CustomLevel { get; }
    public FactionType FactionType { get; }
    public long? TimeSpaceOwnerId { get; }
    public TimeSpaceFileConfiguration TimeSpaceInfo { get; }
    public RainBowFlag RainbowFlag { get; }
}