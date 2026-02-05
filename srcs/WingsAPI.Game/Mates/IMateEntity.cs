using System;
using System.Collections.Generic;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Items;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Mates;

public interface IMateEntity : IBattleEntity, IMateRevivalComponent, IMonsterData
{
    short CloseDefence { get; set; }
    short DistanceDodge { get; set; }
    bool IsSitting { get; set; }
    bool IsUsingSp { get; set; }
    DateTime LastDeath { get; set; }
    DateTime LastDefence { get; set; }
    DateTime LastLowLoyaltyEffect { get; set; }
    DateTime LastHealth { get; set; }
    DateTime LastLoyaltyRecover { get; set; }
    DateTime LastSkillUse { get; set; }
    DateTime LastBasicSkill { get; set; }
    DateTime? SpawnMateByGuardian { get; set; }
    int? SavedGuardianVnum { get; set; }
    IPlayerEntity Owner { get; set; }
    byte PetSlot { get; set; }
    IBattleEntitySkill LastUsedPartnerSkill { get; set; }
    short HitRate { get; set; }
    byte Attack { get; set; }
    bool CanPickUp { get; set; }
    long CharacterId { get; set; }
    byte Defence { get; set; }
    long Experience { get; set; }
    bool IsSummonable { get; set; }
    bool IsTeamMember { get; set; }
    short Loyalty { get; set; }
    short MapX { get; set; }
    short MapY { get; set; }
    short MinilandX { get; set; }
    short MinilandY { get; set; }
    MateType MateType { get; set; }
    string MateName { get; set; }
    int NpcMonsterVNum { get; set; }
    short Skin { get; set; }
    int HitCriticalChance { get; set; }
    int HitCriticalDamage { get; set; }
    bool IsLimited { get; init; }
    byte HeroLevel { get; set; }
    byte Stars { get; set; }
    int TrainerExperience { get; set; }
    bool IsTemporaryPet { get; set; }
    bool HasDhaPremium { get; set; }
    bool IsDhaLootEnabled { get; set; }
    List<int> TrainerSkills { get; set; }
    int HitsAgainstDoll { get; set; }
    int HitsFromDoll { get; set; }
    List<long> SavedDrops { get; set; }
    VisualType TargetVisualType { get; set; }
    long TargetId { get; set; }
    DateTime? SpCooldownEnd { get; set; }
    DateTime LastEffect { get; set; }
    DateTime LastPetUpgradeEffect { get; set; }
    GameItemInstance Weapon { get; }
    GameItemInstance Armor { get; }
    GameItemInstance Gloves { get; }
    GameItemInstance Boots { get; }
    GameItemInstance Specialist { get; }
    SkillInfo BasicSkill { get; }
    IMateStatisticsComponent StatisticsComponent { get; }
    short? SpecialSkillVnum { get; set; }
    void RefreshStatistics();
    void Initialize();
}