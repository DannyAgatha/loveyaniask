// NosEmu
// 


using System;
using System.Collections.Generic;
using WingsAPI.Data.Drops;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.Game._enum;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Npcs;

public class MonsterData : NpcMonsterDto, IMonsterData
{
    // Do not remove!
    public MonsterData()
    {
    }

    public MonsterData(IMonsterData monsterData)
    {
        AmountRequired = monsterData.AmountRequired;
        ArmorLevel = monsterData.ArmorLevel;
        AttackType = monsterData.AttackType;
        AttackUpgrade = monsterData.AttackUpgrade;
        BasicCastTime = monsterData.BasicCastTime;
        BasicCooldown = monsterData.BasicCooldown;
        BasicRange = monsterData.BasicRange;
        AttackEffect = monsterData.AttackEffect;
        BasicHitChance = monsterData.BasicHitChance;
        CanBeCollected = monsterData.CanBeCollected;
        CanBeDebuffed = monsterData.CanBeDebuffed;
        CanBeCaught = monsterData.CanBeCaught;
        CanBePushed = monsterData.CanBePushed;
        CanRegenMp = monsterData.CanRegenMp;
        CanWalk = monsterData.CanWalk;
        CellSize = monsterData.CellSize;
        CleanDamageMin = monsterData.CleanDamageMin;
        CleanDamageMax = monsterData.CleanDamageMax;
        CleanHitRate = monsterData.CleanHitRate;
        CleanMeleeDefence = monsterData.CleanMeleeDefence;
        CleanRangeDefence = monsterData.CleanRangeDefence;
        CleanMagicDefence = monsterData.CleanMagicDefence;
        CleanDodge = monsterData.CleanDodge;
        CleanHp = monsterData.CleanHp;
        CleanMp = monsterData.CleanMp;
        CloseDefence = monsterData.BaseCloseDefence;
        Concentrate = monsterData.BaseConcentrate;
        CriticalChance = monsterData.BaseCriticalChance;
        CriticalRate = monsterData.BaseCriticalRate;
        DamagedOnlyLastJajamaruSkill = monsterData.DamagedOnlyLastJajamaruSkill;
        DamageMaximum = monsterData.BaseDamageMaximum;
        DamageMinimum = monsterData.BaseDamageMinimum;
        DarkResistance = monsterData.BaseDarkResistance;
        DeathEffect = monsterData.DeathEffect;
        MaxHp = monsterData.BaseMaxHp;
        MaxMp = monsterData.BaseMaxMp;
        DefenceDodge = monsterData.DefenceDodge;
        DefenceUpgrade = monsterData.DefenceUpgrade;
        DisappearAfterHitting = monsterData.DisappearAfterHitting;
        DisappearAfterSeconds = monsterData.DisappearAfterSeconds;
        DisappearAfterSecondsMana = monsterData.DisappearAfterSecondsMana;
        DistanceDefence = monsterData.DistanceDefence;
        DistanceDefenceDodge = monsterData.DistanceDefenceDodge;
        Element = monsterData.BaseElement;
        ElementRate = monsterData.BaseElementRate;
        SuggestedFaction = monsterData.SuggestedFaction;
        FireResistance = monsterData.BaseFireResistance;
        GiveDamagePercentage = monsterData.GiveDamagePercentage;
        GroupAttack = monsterData.GroupAttack;
        HasMode = monsterData.HasMode;
        HostilityType = monsterData.RawHostility;
        IconId = monsterData.IconId;
        IsPercent = monsterData.IsPercent;
        JobXp = monsterData.JobXp;
        Level = monsterData.BaseLevel;
        HeroLevel = monsterData.BaseHeroLevel;
        LightResistance = monsterData.BaseLightResistance;
        MagicDefence = monsterData.MagicDefence;
        MagicMpFactor = monsterData.MagicMpFactor;
        MeleeHpFactor = monsterData.MeleeHpFactor;
        Stars = monsterData.Stars;
        MinimumAttackRange = monsterData.MinimumAttackRange;
        Id = monsterData.MonsterVNum;
        Name = monsterData.Name;
        NoticeRange = monsterData.NoticeRange;
        OnDefenseOnlyOnce = monsterData.OnDefenseOnlyOnce;
        PermanentEffect = monsterData.PermanentEffect;
        Race = (byte)monsterData.MonsterRaceType;
        RaceType = monsterData.MonsterRaceSubType;
        RangeDodgeFactor = monsterData.RangeDodgeFactor;
        RespawnTime = (int)(monsterData.BaseRespawnTime.TotalMilliseconds / 100);
        SpawnMobOrColor = monsterData.SpawnMobOrColor;
        Speed = monsterData.BaseSpeed;
        SpriteSize = monsterData.SpriteSize;
        TakeDamages = monsterData.TakeDamages;
        VNumRequired = monsterData.VNumRequired;
        WaterResistance = monsterData.BaseWaterResistance;
        WeaponLevel = monsterData.WeaponLevel;
        WinfoValue = monsterData.WinfoValue;
        Xp = monsterData.Xp;
        MaxTries = monsterData.MaxTries;
        CollectionCooldown = monsterData.CollectionCooldown;
        CollectionDanceTime = monsterData.CollectionDanceTime;
        TeleportRemoveFromInventory = monsterData.TeleportRemoveFromInventory;
        HasDash = monsterData.HasDash;
        BCards = monsterData.BCards;
        ModeBCards = monsterData.ModeBCards;
        Drops = monsterData.Drops;
        MonsterSkills = monsterData.MonsterSkills;
        if (monsterData is NpcMonsterDto npcMonsterDto)
        {
            Skills = npcMonsterDto.Skills;
        }
    }

    public short BaseCloseDefence => CloseDefence;
    public short BaseConcentrate => Concentrate;
    public short BaseCriticalChance => CriticalChance;
    public short BaseCriticalRate => CriticalRate;
    public int BaseDamageMaximum => DamageMaximum;
    public int BaseDamageMinimum => DamageMinimum;
    public short BaseDarkResistance => DarkResistance;
    public int BaseMaxHp => MaxHp;
    public int BaseMaxMp => MaxMp;
    public byte BaseElement => Element;
    public short BaseElementRate => ElementRate;
    public short BaseFireResistance => FireResistance;
    public int RawHostility => HostilityType;
    public byte BaseLevel => Level;
    public byte BaseHeroLevel => HeroLevel;
    public short BaseLightResistance => LightResistance;
    public int MonsterVNum => Id;
    public MonsterRaceType MonsterRaceType => (MonsterRaceType)Race;
    public byte MonsterRaceSubType => RaceType;
    public byte BaseSpeed => Speed;
    public short BaseWaterResistance => WaterResistance;
    public TimeSpan BaseRespawnTime => TimeSpan.FromMilliseconds(RespawnTime * 100);

    public FactionType? SuggestedFaction { get; set; }
    public IReadOnlyList<DropDTO> Drops { get; set; }
    public bool CanSeeInvisible { get; set; }
    public IReadOnlyList<BCardDTO> BCards { get; set; }
    public IReadOnlyList<BCardDTO> ModeBCards { get; set; }
    public IReadOnlyList<INpcMonsterSkill> MonsterSkills { get; set; } = new List<INpcMonsterSkill>();
}