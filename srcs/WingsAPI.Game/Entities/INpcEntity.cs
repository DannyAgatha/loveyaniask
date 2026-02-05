using System;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Maps;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Shops;

namespace WingsEmu.Game.Entities;

public interface INpcEntity : INpcMonsterEntity, INpcAdditionalData
{
    Guid UniqueId { get; }
    bool IsHostile { get; }
    bool CanAttack { get; }
    public int? QuestDialog { get; }
    short Dialog { get; }
    short Effect { get; }
    TimeSpan EffectDelay { get; }
    bool IsMoving { get; }
    bool IsSitting { get; }
    int MapId { get; }
    int NpcVNum { get; }
    ShopNpc ShopNpc { get; set; }
    bool HasGodMode { get; }
    byte CurrentCollection { get; set; }
    DateTime LastCollection { get; set; }
    string CustomName { get; }
    long? CharacterPartnerId { get; set; }
    DateTime LastBasicAttack { get; set; }
    DateTime LastTimeSpaceHeal { get; set; }
    DateTime LastFrozenEffect { get; set; }
    RainBowFlag RainbowFlag { get; set; }
    long? TimeSpaceOwnerId { get; }
    TimeSpaceFileConfiguration TimeSpaceInfo { get; }
    void ChangeMapInstance(IMapInstance mapInstance);
}