using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.MultiLanguage;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Extensions;

public static class NpcMonsterExtension
{
    public static string GenerateEInfo(this IMonsterData npcMonster, IGameLanguageService gameLanguage, RegionLanguageType languageType)
    {
        string name = gameLanguage.GetLanguage(GameDataType.NpcMonster, npcMonster.Name, languageType);

        return "e_info 10 " +
            $"{npcMonster.MonsterVNum} " +
            $"{npcMonster.BaseLevel} " +
            $"{npcMonster.BaseElement} " +
            $"{(byte)npcMonster.AttackType} " +
            $"{npcMonster.BaseElementRate} " +
            $"{npcMonster.AttackUpgrade} " +
            $"{npcMonster.BaseDamageMinimum} " +
            $"{npcMonster.BaseDamageMaximum} " +
            $"{npcMonster.BaseConcentrate} " +
            $"{npcMonster.BaseCriticalChance} " +
            $"{npcMonster.BaseCriticalRate} " +
            $"{npcMonster.DefenceUpgrade} " +
            $"{npcMonster.BaseCloseDefence} " +
            $"{npcMonster.DefenceDodge} " +
            $"{npcMonster.DistanceDefence} " +
            $"{npcMonster.DistanceDefenceDodge} " +
            $"{npcMonster.MagicDefence} " +
            $"{npcMonster.BaseFireResistance} " +
            $"{npcMonster.BaseWaterResistance} " +
            $"{npcMonster.BaseLightResistance} " +
            $"{npcMonster.BaseDarkResistance} " +
            $"{npcMonster.BaseMaxHp} " +
            $"{npcMonster.BaseMaxMp} " +
            "-1 " +
            "@"+
            " 0 0";
    }

    public static string GenerateMonsterEInfo(this IMonsterData npcMonster, IGameLanguageService gameLanguage, RegionLanguageType languageType)
    {
        string name = gameLanguage.GetLanguage(GameDataType.NpcMonster, npcMonster.Name, languageType);
        return "e_info 10 " +
            $"{npcMonster.MonsterVNum} " +
            $"{npcMonster.BaseLevel} " +
            $"{npcMonster.BaseElement} " +
            $"{(byte)npcMonster.AttackType} " +
            $"{npcMonster.BaseElementRate} " +
            $"{npcMonster.AttackUpgrade} " +
            $"{npcMonster.BaseDamageMaximum} " +
            $"{npcMonster.BaseDamageMinimum} " +
            $"{npcMonster.BaseConcentrate} " +
            $"{npcMonster.BaseCriticalChance} " +
            $"{npcMonster.BaseCriticalRate} " +
            $"{npcMonster.DefenceUpgrade} " +
            $"{npcMonster.BaseCloseDefence} " +
            $"{npcMonster.DefenceDodge} " +
            $"{npcMonster.DistanceDefence} " +
            $"{npcMonster.DistanceDefenceDodge} " +
            $"{npcMonster.MagicDefence} " +
            $"{npcMonster.BaseFireResistance} " +
            $"{npcMonster.BaseWaterResistance} " +
            $"{npcMonster.BaseLightResistance} " +
            $"{npcMonster.BaseDarkResistance} " +
            $"{npcMonster.BaseMaxHp} " +
            $"{npcMonster.BaseMaxMp} " +
            "-1 " +
            "@" +
            " 0 0";
    }

    public static void SendNpcInfo(this IClientSession session, IMonsterData npcMonster, IGameLanguageService gameLanguage) =>
        session.SendPacket(npcMonster.GenerateEInfo(gameLanguage, session.UserLanguage));

    public static void SendMonsterInfo(this IClientSession session, IMonsterData npcMonster, IGameLanguageService gameLanguage) =>
        session.SendPacket(npcMonster.GenerateMonsterEInfo(gameLanguage, session.UserLanguage));

    public static int GetModifier(this MonsterData monster)
    {
        return monster.AttackType switch
        {
            AttackType.Melee => monster.MeleeHpFactor,
            AttackType.Ranged => monster.RangeDodgeFactor,
            AttackType.Magical => monster.MagicMpFactor
        };
    }

    public static bool CanHit(this IMonsterData npcMonster, IBattleEntity entity)
    {
        if (entity.IsFrozenByGlacerus())
        {
            return false;
        }
        
        switch (entity)
        {
            case IPlayerEntity player when player.CheatComponent.IsInvisible:
                return false;
            case IMateEntity mateEntity:

                if (mateEntity.Owner == null)
                {
                    break;
                }

                if (mateEntity.Owner.IsInvisible())
                {
                    return false;
                }

                if (mateEntity.Owner.Invisible)
                {
                    return false;
                }

                if (mateEntity.Owner.CheatComponent.IsInvisible)
                {
                    return false;
                }

                if (mateEntity.Owner.IsOnVehicle)
                {
                    return false;
                }

                break;
        }

        return !(npcMonster.MonsterRaceType == MonsterRaceType.People && npcMonster.MonsterRaceSubType == 3 && entity.Faction == FactionType.Angel
            | npcMonster.MonsterRaceType == MonsterRaceType.Angel && entity.Faction == FactionType.Demon);
    }

    public static bool FindPhantomAmulet(this IMonsterData npcMonster, IBattleEntity entity)
    {
        if (npcMonster.RawHostility != (int)HostilityType.ATTACK_NOT_WEARING_PHANTOM_AMULET)
        {
            return false;
        }

        switch (entity)
        {
            case IPlayerEntity character:
                GameItemInstance amulet = character.Amulet;
                if (amulet == null)
                {
                    return false;
                }

                return amulet.GameItem.Id == (short)ItemVnums.PHANTOM_AMULET;
            case IMateEntity mateEntity:
                IPlayerEntity owner = mateEntity.Owner;
                GameItemInstance ownerAmulet = owner.Amulet;
                if (ownerAmulet == null)
                {
                    return false;
                }

                return ownerAmulet.GameItem.Id == (short)ItemVnums.PHANTOM_AMULET;
        }

        return true;
    }

    public static MonsterRaceType GetMonsterRaceType(this IBattleEntity entity)
    {
        return entity switch
        {
            IPlayerEntity => MonsterRaceType.People,
            IMateEntity mateEntity => mateEntity.MonsterRaceType,
            INpcEntity mapNpc => mapNpc.MonsterRaceType,
            IMonsterEntity monster => monster.MonsterRaceType,
            _ => MonsterRaceType.People
        };
    }

    public static Enum GetMonsterRaceSubType(this IMonsterData monster)
    {
        MonsterRaceType monsterRaceType = monster.MonsterRaceType;
        byte subType = monster.MonsterRaceSubType;

        Enum raceSubType;
        switch (monsterRaceType)
        {
            case MonsterRaceType.LowLevel:
                raceSubType = (MonsterSubRace.LowLevel)subType;
                break;
            case MonsterRaceType.HighLevel:
                raceSubType = (MonsterSubRace.HighLevel)subType;
                break;
            case MonsterRaceType.Furry:
                raceSubType = (MonsterSubRace.Furry)subType;
                break;
            case MonsterRaceType.People:
                raceSubType = (MonsterSubRace.People)subType;
                break;
            case MonsterRaceType.Angel:
                raceSubType = (MonsterSubRace.Angels)subType;
                break;
            case MonsterRaceType.Undead:
                raceSubType = (MonsterSubRace.Undead)subType;
                break;
            case MonsterRaceType.Spirit:
                raceSubType = (MonsterSubRace.Spirits)subType;
                break;
            case MonsterRaceType.Other:
                raceSubType = (MonsterSubRace.Other)subType;
                break;
            case MonsterRaceType.Fixed:
                raceSubType = (MonsterSubRace.Fixed)subType;
                break;
            default:
                return null;
        }

        return raceSubType;
    }

    public static string GenerateCMode(this IMonsterEntity monsterEntity, short? value = null) => $"c_mode 3 {monsterEntity.Id} {value ?? monsterEntity.Morph} 0 0";
    public static void BroadcastMonsterMorph(this IMonsterEntity monsterEntity, short? value = null) => monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateCMode(value));

    public static bool IsMonsterSpawningMonstersForQuest(this IMonsterEntity monsterEntity) => monsterEntity.RawHostility > 20000;

    public static string GenerateEnhancedGuri(this IMonsterEntity monsterEntity) => $"guri 21 3 {monsterEntity.Id} 0";
    
    public static string GenerateUntarget(this IMonsterEntity monsterEntity) => $"guri 11 3 {monsterEntity.Id}";
    public static void BroadcastEnhancedGuri(this IMonsterEntity monsterEntity) => monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateEnhancedGuri());

    public static bool IsMandra(this IBattleEntity battleEntity)
    {
        if (battleEntity is not IMonsterEntity monsterEntity)
        {
            return false;
        }

        return monsterEntity.MonsterVNum is >= (short)MonsterVnum.HAPPY_MANDRA and <= (short)MonsterVnum.STRONG_GRASSMANDRA
            or (short)MonsterVnum.GIANT_MANDRA or (short)MonsterVnum.DASHING_MANDRA;
    }

    public static bool IsPhantom(this IMonsterEntity monsterEntity) =>
        (MonsterVnum)monsterEntity.MonsterVNum is MonsterVnum.RUBY_PHANTOM or MonsterVnum.EMERALD_PHANTOM or MonsterVnum.SAPPHIRE_PHANTOM;

    public static bool IsPhantom(this INpcEntity npcEntity) =>
        (MonsterVnum)npcEntity.MonsterVNum is MonsterVnum.RUBY_SHADOW_PHANTOM or MonsterVnum.EMERALD_SHADOW_PHANTOM or MonsterVnum.SAPPHIRE_SHADOW_PHANTOM;

    public static async Task TryDespawnTimeSpacePortal(this IClientSession session)
    {
        INpcEntity timeSpacePortal = session.CurrentMapInstance.GetPassiveNpcs().FirstOrDefault(x => x.TimeSpaceOwnerId.HasValue && x.TimeSpaceOwnerId.Value == session.PlayerEntity.Id);
        if (timeSpacePortal == null)
        {
            return;
        }

        string firstEffectPacket = timeSpacePortal.GenerateEffectGround(EffectType.BlueTimeSpace, timeSpacePortal.PositionX, timeSpacePortal.PositionY, true);
        string secondEffectPacket = timeSpacePortal.GenerateEffectGround(EffectType.BlueRemoveTimeSpace, timeSpacePortal.PositionX, timeSpacePortal.PositionY, false);

        timeSpacePortal.MapInstance.Broadcast(_ => firstEffectPacket);
        timeSpacePortal.MapInstance.Broadcast(_ => secondEffectPacket);

        await timeSpacePortal.EmitEventAsync(new MapLeaveNpcEntityEvent(timeSpacePortal));
    }
}