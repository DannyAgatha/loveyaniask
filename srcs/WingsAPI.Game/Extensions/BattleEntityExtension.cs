using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Extensions;

public static class BattleEntityExtension
{
    private static IRandomGenerator _randomGenerator => StaticRandomGenerator.Instance;
    
    /// <summary>
    ///     Check if the BattleEntity is in the given Range.
    /// </summary>
    /// <param name="battleEntity">The BattleEntity from which position check</param>
    /// <param name="mapX">The X coordinate on the Map of the object to check.</param>
    /// <param name="mapY">The Y coordinate on the Map of the object to check.</param>
    /// <param name="distance">The maximum distance of the object to check.</param>
    /// <returns>True if the BattleEntity is in range, False if not.</returns>
    public static bool IsInRange(this IBattleEntity battleEntity, short mapX, short mapY, byte distance) => battleEntity.Position.GetDistance(mapX, mapY) <= distance;

    public static bool IsInvisible(this IBattleEntity seen)
    {
        if (seen is IPlayerEntity player && player.CheatComponent.IsInvisible)
        {
            return true;
        }

        return seen.BCardComponent.HasBCard(BCardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide)
            || seen.BCardComponent.HasBCard(BCardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Hide)
            || seen.BCardComponent.HasBCard(BCardType.FalconSkill, (byte)AdditionalTypes.FalconSkill.Ambush)
            || seen.BCardComponent.HasBCard(BCardType.LordBerios, (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence)
            || seen.BuffComponent.HasBuff((short)BuffVnums.SWARM_OF_BATS);
    }

    public static bool HasGodMode(this IBattleEntity entity)
    {
        if (entity is IPlayerEntity player && player.CheatComponent.HasGodMode)
        {
            return true;
        }

        if (entity.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.DisableHPConsumption))
        {
            return true;
        }

        if (entity.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.NeverReceiveDamage))
        {
            return true;
        }
        
        if (entity.BCardComponent.HasBCard(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.EarnUltimatePointsOnBlock))
        {
            return true;
        }
        
        if (entity.BCardComponent.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReceiveWhenBlockingWithBuffActive))
        {
            return true;
        }

        return entity.BCardComponent.HasBCard(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.NoHPConsumption);
    }

    public static int ApplyCooldownReduction(this IBattleEntity battleEntity, SkillInfo skill)
    {
        short cooldown = skill.Cooldown;
        if (battleEntity.NoCooldown())
        {
            return 0;
        }

        if (ResetByFairyWings(battleEntity, skill))
        {
            return 0;
        }
        
        if (battleEntity.BCardComponent.HasBCard(BCardType.SpecialDamageAndExplosions, (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR) || 
            skill.BCards.Any(b => b.Type == (int)BCardType.SpecialDamageAndExplosions && b.SubType == (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR))
        {
            return cooldown;
        }
        
        if (battleEntity.BuffComponent.HasBuff((int)BuffVnums.OPPORTUNITY_TO_ATTACK) && skill.Vnum == (int)SkillsVnums.LOTUS_LEAP)
        {
            battleEntity.RemoveBuffAsync((int)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
            return 0;
        }
        
        if (battleEntity is IPlayerEntity player && player.UsageSkillWithoutCd != 0)
        {
            player.UsageSkillWithoutCd--;
            return 0;
        }
        
        (int firstData, int _) cooldownDecrease =
            battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.SkillCooldownDecreased, battleEntity.Level);
        
        (int firstData, int _) cooldownIncrease =
            battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.SkillCooldownIncreased, battleEntity.Level);
        
        if (skill.AttackType is AttackType.Magical)
        {
            int magicalCooldownDecrease = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownDecreased, battleEntity.Level).firstData;
            cooldownDecrease.firstData -= (100 - magicalCooldownDecrease);
            
            int magicalCooldownIncrease = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.AbsorbedSpirit, (byte)AdditionalTypes.AbsorbedSpirit.MagicCooldownIncreased, battleEntity.Level).firstData;
            cooldownIncrease.firstData += (100 - magicalCooldownIncrease);
        }
        
        double increaseMultiplier = cooldownIncrease.firstData * 0.01;
        double decreaseMultiplier = cooldownDecrease.firstData * 0.01;
        
        double newCooldown = cooldown * increaseMultiplier * decreaseMultiplier;
        
        if (battleEntity is IPlayerEntity { SubClass: SubClassType.DarkNecromancer } playerEntity && playerEntity.Session.CurrentMapInstance.IsPvp)
        {
            double cooldownReductionRate = playerEntity.TierLevel switch
            {
                1 => 0.05, // Reduce by 5%
                2 => 0.06, // Reduce by 6%
                3 => 0.07, // Reduce by 7%
                4 => 0.08, // Reduce by 8%
                5 => 0.09, // Reduce by 9%
                _ => 0     // No reduction if the level is not specified
            };
            
            newCooldown *= 1 - cooldownReductionRate;
        }
        
        cooldown = (short)newCooldown;

        return cooldown < 0 ? 0 : cooldown;
    }

    private static bool ResetByFairyWings(IBattleEntity battleEntity, SkillInfo skill)
    {
        if (!battleEntity.BCardComponent.HasBCard(BCardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.CooldownResetChance))
        {
            return false;
        }
        
        if (battleEntity.BCardComponent.HasBCard(BCardType.SpecialDamageAndExplosions, (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR) ||
            skill.BCards.Any(b => b.Type == (int)BCardType.SpecialDamageAndExplosions && b.SubType == (byte)AdditionalTypes.SpecialDamageAndExplosions.NotAffectedByCDR))
        {
            return false;
        }
        
        if (skill.TargetAffectedEntities != TargetAffectedEntities.Enemies)
        {
            return false;
        }

        if (skill.CastId == 0)
        {
            return false;
        }

        if (battleEntity is not IPlayerEntity playerEntity)
        {
            return false;
        }

        int firstData = playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.CooldownResetChance, playerEntity.Level).firstData;
        if (StaticRandomGenerator.Instance.RandomNumber() > firstData)
        {
            return false;
        }

        if (!playerEntity.SkillComponent.SkillsResets.TryGetValue(skill.Vnum, out byte usages))
        {
            usages = 0;
        }

        if (playerEntity.SkillComponent.LastSkillVnum.HasValue && playerEntity.SkillComponent.LastSkillVnum.Value == skill.Vnum)
        {
            usages += 1;
        }
        else
        {
            playerEntity.SkillComponent.SkillsResets.Clear();
        }

        if (usages > 1)
        {
            return false;
        }

        playerEntity.SkillComponent.LastSkillVnum = skill.Vnum;
        playerEntity.SkillComponent.SkillsResets[skill.Vnum] = usages;
        playerEntity.BroadcastEffectInRange(EffectType.FairyResetBuff);

        return true;
    }

    private static bool NoCooldown(this IBattleEntity entity) => entity is IPlayerEntity character && character.CheatComponent.HasNoCooldown;

    public static IEnumerable<IBattleEntity> GetEnemiesInRange(this IBattleEntity sender, IBattleEntity caster, short range) => sender.Position.GetEnemiesInRange(caster, range);

    public static IEnumerable<IBattleEntity> GetAlliesInRange(this IBattleEntity sender, IBattleEntity caster, short range) => sender.Position.GetAlliesInRange(caster, range);

    public static List<Position> GetCellsInLine(this Position pos1, Position pos2)
    {
        var cells = new List<Position>();

        short x0 = pos1.X;
        short y0 = pos1.Y;

        short x1 = pos2.X;
        short y1 = pos2.Y;

        int dx = Math.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;

        int dy = -Math.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;

        int err = dx + dy;

        while (true)
        {
            cells.Add(new Position(x0, y0));
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += (short)sx;
            }

            if (e2 > dx)
            {
                continue;
            }

            err += dx;
            y0 += (short)sy;
        }

        return cells;
    }

    public static void ChangeSize(this IBattleEntity battleEntity, int size)
    {
        battleEntity.Size = size;
        battleEntity.MapInstance?.Broadcast(battleEntity.GenerateScal());
    }

    public static int GetHpPercentage(this IBattleEntity target)
    {
        if (target.Hp == 0 || target.MaxHp == 0)
        {
            return 0;
        }

        int hpPercentage;
        
        if (target is IMonsterEntity monster)
        {
            hpPercentage = (int)(monster.Hp / (float)(monster.FakeMaxHp ?? monster.MaxHp) * 100);
        }
        else
        {
            hpPercentage = (int)(target.Hp / (float)target.MaxHp * 100);
        }
        return hpPercentage == 0 ? 1 : hpPercentage;
    }
    
    public static int GetMissingHpPercentage(this IBattleEntity target)
    {
        if (target.Hp == 0 || target.MaxHp == 0)
        {
            return 0;
        }

        return 100 - target.GetHpPercentage();
    }

    public static bool ShouldSendScal(this IBattleEntity battleEntity) => battleEntity.Size != 10;

    public static string GenerateScal(this IBattleEntity battleEntity) => $"char_sc {((byte)battleEntity.Type).ToString()} {battleEntity.Id.ToString()} {battleEntity.Size.ToString()}";

    public static string GenerateSelfEffect(this IBattleEntity battleEntity, EffectType effectVnum) => $"eff_s {(byte)battleEntity.Type} {battleEntity.Id} {(short)effectVnum} 0";

    public static string GenerateEffectPacket(this IBattleEntity battleEntity, int effectVnum) => $"eff {(byte)battleEntity.Type} {battleEntity.Id} {effectVnum}";

    public static string GenerateEffectPacket(this IBattleEntity battleEntity, EffectType effectType) => battleEntity.GenerateEffectPacket((int)effectType);

    public static void BroadcastEffectInRange(this IBattleEntity entity, EffectType effectType)
    {
        if (entity.IsInvisibleGm())
        {
            return;
        }

        entity.MapInstance.Broadcast(entity.GenerateEffectPacket((int)effectType), new RangeBroadcast(entity.PositionX, entity.PositionY));
    }

    public static void BroadcastEffectInRange(this IBattleEntity entity, int effectId)
    {
        if (entity.IsInvisibleGm())
        {
            return;
        }

        entity.MapInstance.Broadcast(entity.GenerateEffectPacket(effectId), new RangeBroadcast(entity.PositionX, entity.PositionY));
    }

    public static string GenerateMvPacket(this IBattleEntity entity, int speed = default)
        => $"mv {(byte)entity.Type} {entity.Id} {entity.PositionX} {entity.PositionY} {(speed == default ? entity.Speed : speed).ToString()}";

    public static string GenerateStPacket(this IBattleEntity target)
    {
        return
            $"st {(byte)target.Type} {target.Id} {target.Level} {target switch
            {
                IPlayerEntity character => character.HeroLevel,
                IMateEntity mate => mate.HeroLevel,
                IMonsterEntity monster => monster.HeroLevel,
                INpcEntity npc => npc.HeroLevel,
            }} {target.HpPercentage} {target.MpPercentage} {target.Hp} {target.Mp} {(target is IMonsterEntity npcMonster ? npcMonster.FakeMaxHp ?? npcMonster.MaxHp : target.MaxHp)} {target.MaxMp} 0 {target.BuffComponent.GetAllBuffs().Aggregate(string.Empty, (current, buff) => current + $" {buff.CardId}.{buff.CasterLevel}")}";
    }

    public static string GenerateTeleportPacket(this IBattleEntity entity, short x, short y) => $"tp {(byte)entity.Type} {entity.Id} {x} {y} 0";

    public static void TeleportOnMap(this IBattleEntity entity, short x, short y, bool teleportMates = false)
    {
        var newPosition = new Position(x, y);
        switch (entity)
        {
            case IPlayerEntity playerEntity:

                if (!teleportMates)
                {
                    entity.ChangePosition(newPosition);
                    entity.MapInstance.Broadcast(entity.GenerateTeleportPacket(x, y));
                    return;
                }

                IReadOnlyList<IMateEntity> nosMate = playerEntity.MateComponent.TeamMembers();
                foreach (IMateEntity mate in nosMate)
                {
                    mate.ChangePosition(newPosition);
                    playerEntity.MapInstance.Broadcast(mate.GenerateTeleportPacket(x, y));
                }

                break;
        }

        entity.ChangePosition(newPosition);
        entity.MapInstance.Broadcast(entity.GenerateTeleportPacket(x, y));
    }

    public static int GetMpPercentage(this IBattleEntity target)
    {
        if (target.Mp == 0 || target.MaxMp == 0)
        {
            return 0;
        }

        int mpPercentage = (int)(target.Mp / (float)target.MaxMp * 100);
        return mpPercentage == 0 ? 1 : mpPercentage;
    }

    public static bool IsPvpActivated(this IBattleEntity entity, IBattleEntity target)
    {
        if (!target.IsPlayer() && !target.IsMate())
        {
            return false;
        }

        if (entity.Id == target.Id && entity.Type == target.Type)
        {
            return false;
        }

        if (entity.BuffComponent.HasBuff((short)BuffVnums.PVP) && !entity.MapInstance.IsPvp)
        {
            // BIGU SWITCHU 2021 KEKW

            // 1 -> when target is Player (A) and A have PvP buff and entity isn't in the same group with A
            // 2 -> when target is Mate (B) and owner (C) have PvP buff and entity isn't in the same group with C
            // 3 -> when caster is Mate and target is Player (D) and Mate Owner (E) have PvP buff and D have PvP buff and E isn't in the same group with D
            // 4 => when caster is Mate (F - Owner) and target is Mate (G - Owner) and F and G have PvP buff and F isn't in the same group with G

            return entity switch
            {
                IPlayerEntity character when target is IPlayerEntity characterTarget =>
                    characterTarget.BuffComponent.HasBuff((short)BuffVnums.PVP) && character.IsInGroupOf(characterTarget) == false,

                IPlayerEntity character when target is IMateEntity mateTarget =>
                    mateTarget.Owner.Id != character.Id && mateTarget.Owner.BuffComponent.HasBuff((short)BuffVnums.PVP) && character.IsInGroupOf(mateTarget.Owner) == false,

                IMateEntity mate when target is IPlayerEntity character =>
                    mate.Owner.Id != character.Id && character.BuffComponent.HasBuff((short)BuffVnums.PVP) && mate.Owner.BuffComponent.HasBuff((short)BuffVnums.PVP)
                    && mate.Owner.IsInGroupOf(character) == false,

                IMateEntity mate when target is IMateEntity mateTarget =>
                    mate.Owner.Id != mateTarget.Owner.Id && mate.Owner.BuffComponent.HasBuff((short)BuffVnums.PVP) && mateTarget.Owner.BuffComponent.HasBuff((short)BuffVnums.PVP)
                    && mate.Owner.IsInGroupOf(mateTarget.Owner) == false,

                _ => false
            };
        }
        
        if (entity.MapInstance.IsAct6PvpInstance)
        {
            return entity switch
            {
                IPlayerEntity character when target is IPlayerEntity characterTarget => character.Faction != characterTarget.Faction,
                IPlayerEntity character when target is IMateEntity mateTarget => character.Faction != mateTarget.Faction,
                IMateEntity mate when target is IPlayerEntity character => mate.Faction != character.Faction,
                IMateEntity mate when target is IMateEntity mateTarget => mate.Faction != mateTarget.Faction,
                _ => false
            };
        }

        if (entity.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
        {
            switch (entity)
            {
                case IPlayerEntity character when target is IPlayerEntity characterTarget:

                    RainbowBattleParty rainbowParty = character.RainbowBattleComponent.RainbowBattleParty;

                    return rainbowParty is { Started: true, FinishTime: null }
                        && !characterTarget.RainbowBattleComponent.IsFrozen && !character.RainbowBattleComponent.IsFrozen
                        && characterTarget.RainbowBattleComponent.Team != character.RainbowBattleComponent.Team;
                case IPlayerEntity character when target is IMateEntity mateTarget:

                    RainbowBattleParty rainbowPartyTwo = character.RainbowBattleComponent.RainbowBattleParty;

                    return rainbowPartyTwo is { Started: true, FinishTime: null }
                        && mateTarget.Owner.Id != character.Id && !mateTarget.Owner.RainbowBattleComponent.IsFrozen
                        && !character.RainbowBattleComponent.IsFrozen && mateTarget.Owner.RainbowBattleComponent.Team != character.RainbowBattleComponent.Team;
                case IMateEntity mate when target is IPlayerEntity character:

                    RainbowBattleParty rainbowPartyThree = mate.Owner.RainbowBattleComponent.RainbowBattleParty;

                    return rainbowPartyThree is { Started: true, FinishTime: null }
                        && !mate.Owner.RainbowBattleComponent.IsFrozen && !character.RainbowBattleComponent.IsFrozen
                        && mate.Owner.Id != character.Id && mate.Owner.RainbowBattleComponent.Team != character.RainbowBattleComponent.Team;
                case IMateEntity mate when target is IMateEntity mateTarget:

                    RainbowBattleParty rainbowPartyFour = mate.Owner.RainbowBattleComponent.RainbowBattleParty;

                    return rainbowPartyFour is { Started: true, FinishTime: null }
                        && !mate.Owner.RainbowBattleComponent.IsFrozen && !mateTarget.Owner.RainbowBattleComponent.IsFrozen
                        && mate.Owner.Id != mateTarget.Owner.Id && mate.Owner.RainbowBattleComponent.Team != mateTarget.Owner.RainbowBattleComponent.Team;
                default:
                    return false;
            }
        }

        if (entity.MapInstance.MapInstanceType == MapInstanceType.Alzanor)
        {
            switch (entity)
            {
                case IPlayerEntity character when target is IPlayerEntity characterTarget:

                    AlzanorParty bufferParty = character.AlzanorComponent.AlzanorParty;

                    return bufferParty is { Started: true, FinishTime: null }
                        && characterTarget.AlzanorComponent.Team != character.AlzanorComponent.Team
                        && !characterTarget.ArenaImmunity.HasValue;

                case IPlayerEntity character when target is IMateEntity mateTarget:

                    AlzanorParty bufferPartyTwo = character.AlzanorComponent.AlzanorParty;

                    return bufferPartyTwo is { Started: true, FinishTime: null }
                        && mateTarget.Owner.Id != character.Id && mateTarget.Owner.AlzanorComponent.Team != character.AlzanorComponent.Team;
                case IMateEntity mate when target is IPlayerEntity character:

                    AlzanorParty bufferPartyThree = mate.Owner.AlzanorComponent.AlzanorParty;

                    return bufferPartyThree is { Started: true, FinishTime: null }
                        && mate.Owner.Id != character.Id && mate.Owner.AlzanorComponent.Team != character.AlzanorComponent.Team;

                case IMateEntity mate when target is IMateEntity mateTarget:

                    AlzanorParty bufferPartyFour = mate.Owner.AlzanorComponent.AlzanorParty;

                    return bufferPartyFour is { Started: true, FinishTime: null }
                        && mate.Owner.Id != mateTarget.Owner.Id && mate.Owner.AlzanorComponent.Team != mateTarget.Owner.AlzanorComponent.Team;
                default:
                    return false;
            }
        }

        switch (entity.MapInstance.IsPvp)
        {
            case false:
            case true when !target.IsInPvpZone():
                return false;
        }

        if (entity.MapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return entity.Faction != target.Faction;
        }

        return entity switch
        {
            IPlayerEntity character when target is IPlayerEntity characterTarget
                => !character.ArenaImmunity.HasValue && !characterTarget.ArenaImmunity.HasValue && character.IsInGroupOf(characterTarget) == false,

            IPlayerEntity character when target is IMateEntity mateTarget
                => mateTarget.Owner.IsInPvpZone() && !character.ArenaImmunity.HasValue && !mateTarget.Owner.ArenaImmunity.HasValue && mateTarget.Owner.Id != character.Id &&
                character.IsInGroupOf(mateTarget.Owner) == false,

            IMateEntity mate when target is IPlayerEntity character
                => mate.Owner.IsInPvpZone() && !character.ArenaImmunity.HasValue && !mate.Owner.ArenaImmunity.HasValue && mate.Owner.Id != character.Id && mate.Owner.IsInGroupOf(character) == false,

            IMateEntity mate when target is IMateEntity mateTarget
                => mateTarget.Owner.IsInPvpZone() && mate.Owner.IsInPvpZone() && !mateTarget.Owner.ArenaImmunity.HasValue && !mate.Owner.ArenaImmunity.HasValue &&
                mate.Owner.Id != mateTarget.Owner.Id && mate.Owner.IsInGroupOf(mateTarget.Owner) == false,
            _ => false
        };
    }

    public static bool IsEnemyWith(this IBattleEntity entity, IBattleEntity target)
    {
        if (entity.IsSameEntity(target))
        {
            return false;
        }

        switch (target)
        {
            case IPlayerEntity playerEntity:
                if (playerEntity.IsSeal)
                {
                    return false;
                }

                break;
            case IMateEntity mateEntity:
                if (mateEntity.Owner.IsOnVehicle)
                {
                    return false;
                }

                break;
            case IMonsterEntity monster:
                if (monster.IsJumping)
                {
                    return false;
                }

                if (monster.MonsterVNum == (short)MonsterVnum.ONYX_MONSTER)
                {
                    return false;
                }

                break;
        }

        switch (entity)
        {
            case IPlayerEntity:
                if (target is not IMonsterEntity monsterEntitySummoner)
                {
                    return target.IsMonster() || entity.IsPvpActivated(target);
                }

                if (entity.Id == monsterEntitySummoner.SummonerId && entity.Type == monsterEntitySummoner.SummonerType && !monsterEntitySummoner.IsMateTrainer && !monsterEntitySummoner.IsSparringMonster)
                {
                    return false;
                }

                if (monsterEntitySummoner.SummonerType is VisualType.Player && !monsterEntitySummoner.IsMateTrainer && !monsterEntitySummoner.IsSparringMonster)
                {
                    if (!monsterEntitySummoner.MapInstance.IsPvp || !monsterEntitySummoner.SummonerId.HasValue)
                    {
                        return false;
                    }

                    IPlayerEntity player = monsterEntitySummoner.MapInstance.GetCharacterById(monsterEntitySummoner.SummonerId.Value);
                    return player is null || player.IsPvpActivated(entity);
                }

                if (monsterEntitySummoner.Faction == entity.Faction && monsterEntitySummoner.MapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    return false;
                }

                return target.IsMonster() || entity.IsPvpActivated(target);
            case IMateEntity mate:
                if (target is not IMonsterEntity monsterSummoner)
                {
                    return entity.IsPvpActivated(target);
                }

                if (monsterSummoner.IsMateTrainer)
                {
                    return true;
                }
                
                if (monsterSummoner.IsSparringMonster)
                {
                    return true;
                }

                int mateOwnerId = mate.Owner.Id;
                if (monsterSummoner.SummonerType is not VisualType.Player)
                {
                    return true;
                }

                if (!monsterSummoner.SummonerId.HasValue)
                {
                    return true;
                }

                if (!monsterSummoner.MapInstance.IsPvp)
                {
                    return monsterSummoner.SummonerId.Value != mateOwnerId;
                }

                IPlayerEntity playerMate = monsterSummoner.MapInstance.GetCharacterById(monsterSummoner.SummonerId.Value);
                return playerMate is null || playerMate.IsPvpActivated(mate.Owner);
        }

        if (entity.IsNpc())
        {
            return !(target.IsNpc() || target.IsPlayer() || target.IsMate());
        }

        if (entity is not IMonsterEntity mob)
        {
            return false;
        }

        if (mob.MapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            if (target.Faction == mob.Faction)
            {
                return false;
            }
        }

        if (mob.IsSparringMonster || mob.IsMateTrainer || !mob.SummonerId.HasValue || !mob.SummonerType.HasValue || mob.MonsterVNum == (short)MonsterVnum.ONYX_MONSTER)
        {
            return target.IsPlayer() || target.IsNpc() || target.IsMate() || target.CheckIfIsSummonedMonster(mob);
        }

        if (mob.SummonerId == target.Id && mob.SummonerType == target.Type)
        {
            return false;
        }

        if (target is IMonsterEntity monsterEntity)
        {
            if (mob.SummonerId == monsterEntity.SummonerId)
            {
                return false;
            }

            if (monsterEntity.IsMateTrainer)
            {
                return false;
            }
            
            if (monsterEntity.IsSparringMonster)
            {
                return false;
            }

            if (monsterEntity.SummonerType is null or VisualType.Monster && mob.SummonerType is null or VisualType.Monster)
            {
                return false;
            }
        }

        IBattleEntity summoner = mob.MapInstance?.GetBattleEntity(mob.SummonerType.Value, mob.SummonerId.Value);
        if (summoner == null && mob.SummonerType.Value == VisualType.Player)
        {
            return false;
        }

        return summoner switch
        {
            IPlayerEntity character => character.IsEnemyWith(target),
            _ => target.IsPlayer() || target.IsMate() || target.IsMonster() || target.IsNpc()
        };
    }

    public static bool CanMonsterBeAttacked(this IBattleEntity entity, IMonsterEntity monsterEntity)
    {
        if (monsterEntity.SummonerType is not VisualType.Player)
        {
            return true;
        }

        if (monsterEntity.SummonerId is null)
        {
            return true;
        }

        IPlayerEntity summoner = monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value);
        return summoner == null || summoner.IsEnemyWith(entity);
    }

    private static bool CheckIfIsSummonedMonster(this IBattleEntity entity, IMonsterEntity monster)
    {
        if (entity is not IMonsterEntity monsterEntity)
        {
            return false;
        }

        if (monsterEntity.SummonerType is not VisualType.Player)
        {
            return false;
        }

        if (monster.SummonerType == monsterEntity.SummonerType && monsterEntity.SummonerId == monster.SummonerId)
        {
            return false;
        }

        IPlayerEntity player = monster.SummonerId.HasValue ? entity.MapInstance.GetCharacterById(monster.SummonerId.Value) : null;
        return player == null || player.IsEnemyWith(monster);
    }

    public static bool IsAllyWith(this IBattleEntity entity, IBattleEntity target)
    {
        if (target.IsMate())
        {
            if (target is IMateEntity mateEntity)
            {
                if (mateEntity.Owner.IsOnVehicle)
                {
                    return false;
                }

                if (mateEntity.MapInstance is { MapInstanceType: MapInstanceType.RaidInstance })
                {
                    return true;
                }
            }
        }
        
        if (target.IsPlayer())
        {
            if (target is IPlayerEntity playerEntity && playerEntity.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
            {
                return true;
            }
        }

        if (entity.IsPlayer())
        {
            if (target is IMonsterEntity monster)
            {
                if (monster.Faction == entity.Faction && monster.MapInstance != null && monster.MapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    return true;
                }
            }

            return !target.IsMonster() && (target.IsNpc() || entity.IsPvpActivated(target) == false);
        }
        
        if (entity.IsMate())
        {
            if (target is IMonsterEntity monster)
            {
                if (monster.Faction == entity.Faction && monster.MapInstance != null && monster.MapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    return true;
                }
            }

            return !target.IsMonster() && (target.IsNpc() || entity.IsPvpActivated(target) == false);
        }

        if (entity.IsMonster())
        {
            var owner = entity as IMonsterEntity;
            if (owner?.SummonerType != VisualType.Player || owner.MonsterVNum != 2349)
            {
                return target.IsMonster();
            }

            if (target is IMonsterEntity mob && mob.SummonerId.HasValue && owner.SummonerId.HasValue && owner.SummonerId.Value == mob.SummonerId.Value)
            {
                return true;
            }
            
            if (target is not IPlayerEntity playerEntity)
            {
                return false;
            }
            
            if (!owner.SummonerId.HasValue)
            {
                return false;
            }
            
            if (playerEntity.Id == owner.SummonerId.Value)
            {
                return true;
            }
            
            if (playerEntity.IsInGroup() && playerEntity.GetGroup().Members.Any(s => s.Id == owner.SummonerId.Value))
            {
                return true;
            }

            return target.IsMonster();
        }

        if (!entity.IsNpc())
        {
            return false;
        }

        if (target is not IMonsterEntity mons)
        {
            return true;
        }

        if (mons.IsMateTrainer)
        {
            return true;
        }

        return mons.SummonerType is VisualType.Player;
    }

    public static string GenerateHawkPacket(this IBattleEntity target) => $"hawk_s {(byte)target.Type} {target.Id}";

    public static void BroadcastCastPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skillInfo)
    {
        caster.MapInstance.Broadcast(caster.GenerateCtPacket(target, skillInfo));
    }

    public static string GenerateCtPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill) =>
        $"ct {(byte)caster.Type} {caster.Id} {(target == null ? 0 : (byte)target.Type)} {target?.Id ?? -1} {(skill.CastAnimation == 0 ? -1 : skill.CastAnimation)} {(skill.CastEffect == 0 ? -1 : skill.CastEffect)} {skill.Vnum}";

    public static string GenerateCleanSuPacket(this IBattleEntity entity, IBattleEntity target, int damage, bool isReflect = false) =>
    $"su {(byte)entity.Type} {entity.Id} {(byte)target.Type} {target.Id} -1 0 -1 0 0 0 {(target.IsAlive() ? 1 : 0)} {target.HpPercentage} {damage} 0 {(isReflect ? 1 : 0)} {target.Hp} {target.MaxHp}";

    public static string GenerateSuCapturePacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, bool failed) =>
        $"su {(byte)caster.Type} {caster.Id} {(byte)target.Type} {target.Id} {skill.Vnum} {skill.Cooldown} {(failed ? 16 : 15)} -1 -1 -1 1 0 0 -1 0";

    public static string GenerateSuPacket(this IBattleEntity caster, IBattleEntity target, int damage, SuPacketHitMode hitMode, EffectType hitEffect, bool isReflection)
    {
        return "su " +
            $"{(byte)caster.Type} " +
            $"{caster.Id} " +
            $"{(byte)target.Type} " +
            $"{target.Id} " +
            $"-1 " +
            $"0 " +
            $"-1 " +
            $"{(short)hitEffect} " +
            "-1 " +
            "-1 " +
            $"{(target.IsAlive() ? 1 : 0)} " +
            $"{target.HpPercentage} " +
            $"{damage} " +
            $"{(sbyte)hitMode} " +
            $"{(isReflection ? 1 : 0)} " +
            $"{target.Hp} " +
            $"{target.MaxHp}";
    }
    public static string GenerateSuPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, int damages, SuPacketHitMode hitMode, Position position, bool isReflection, bool isFirst)
    {
        bool isArchMageSkill = skill.Vnum == (short)SkillsVnums.HOLY_EXPLOSION;

        return "su " +
            $"{(byte)caster.Type} " +
            $"{caster.Id} " +
            $"{(byte)target.Type} " +
            $"{target.Id} " +
            $"{(isReflection ? -1 : skill.Vnum)} " +
            $"{(isReflection ? 0 : skill.Cooldown)} " +
            $"{(isReflection || skill.Vnum == (short)SkillsVnums.SPY_OUT_SKILL ? -1 : skill.HitAnimation)} " +
            $"{(isReflection && isArchMageSkill ? 1047 : skill.HitEffect)} " +
            "0 " +
            "0 " +
            $"{(target.IsAlive() ? 1 : 0)} " +
            $"{target.HpPercentage} " +
            $"{damages} " +
            $"{(sbyte)hitMode} " +
            $"{(isReflection ? 1 : 0)} " +
            $"{target.Hp} " +
            $"{(target is IMonsterEntity npcMonster ? npcMonster.FakeMaxHp ?? npcMonster.MaxHp : target.MaxHp)}";
    }
    
    public static string GenerateNoEntitySuPacket(this IBattleEntity caster, IBattleEntity target, int damages,
        SuPacketHitMode hitMode, EffectType effectType) =>
        "su " +
        $"{(byte)caster.Type} " +
        $"{caster.Id} " +
        $"{(byte)target.Type} " +
        $"{target.Id} " +
        "-1 " +
        "0 " +
        "-1 " +
        $"{(short)effectType} " +
        "-1 " +
        "-1 " +
        $"{(target.IsAlive() ? 1 : 0)} " +
        $"{target.HpPercentage} " +
        $"{damages} " +
        $"{(sbyte)hitMode} " +
        "0";
    
    public static string GenerateZephyrPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, int damages) =>
        "su " +
        $"{(byte)caster.Type} " +
        $"{caster.Id} " +
        $"{(byte)target.Type} " +
        $"{target.Id} " +
        "-1 " +
        "0 " +
        $"{skill.HitAnimation} " +
        "4211 " +
        "-1 " +
        "-1 " +
        $"{(target.IsAlive() ? 1 : 0)} " +
        $"{target.HpPercentage} " +
        $"{damages} " +
        "0 " +
        "1 ";
    
    public static void BroadcastZephyrPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, int damages)
    {
        caster.MapInstance.Broadcast(caster.GenerateZephyrPacket(target, skill, damages));
    }

    public static string GenerateSuDashPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, int damages, SuPacketHitMode hitMode, bool isReflection = false, bool isFirst = false)
    {
        if (isReflection)
        {
            return "su " +
                $"{(byte)caster.Type} " +
                $"{caster.Id} " +
                $"{(byte)target.Type} " +
                $"{target.Id} " +
                $"{(isFirst ? -1 : skill.Vnum)} " +
                "0 " +
                "-1 " +
                $"{(isFirst ? skill.HitEffect : -1)} " +
                "-1 " +
                "-1 " +
                $"{(target.IsAlive() ? 1 : 0)} " +
                $"{target.GetHpPercentage()} " +
                $"{damages} " +
                $"{(sbyte)hitMode} " +
                "1";
        }

        return "su " +
            $"{(byte)caster.Type} " +
            $"{caster.Id} " +
            $"{(byte)target.Type} " +
            $"{target.Id} " +
            $"{(!isFirst ? -1 : skill.Vnum)} " +
            $"{skill.Cooldown} " +
            $"{(isFirst ? skill.HitAnimation : -1)} " +
            $"{(isFirst ? skill.HitEffect : -1)} " +
            $"{(isFirst ? caster.PositionX : -1)} " +
            $"{(isFirst ? caster.PositionY : -1)} " +
            $"{(target.IsAlive() ? 1 : 0)} " +
            $"{target.GetHpPercentage()} " +
            $"{damages} " +
            $"{(sbyte)hitMode} " +
            $"{0}";
    }

    public static string GenerateIcon(this IBattleEntity entity, bool isItem, int vnum) => $"icon {(byte)entity.Type} {entity.Id} {(isItem ? 1 : 2)} {vnum}";

    public static void SendIconPacket(this IBattleEntity entity, bool isItem, int vnum)
    {
        switch (entity)
        {
            case IPlayerEntity character:
                character.Session.SendPacket(character.GenerateIcon(isItem, vnum));
                break;
            case IMateEntity mateEntity:
                mateEntity.Owner?.Session.SendPacket(mateEntity.GenerateIcon(isItem, vnum));
                break;
        }
    }

    public static void BroadcastCastNonTarget(this IBattleEntity caster, SkillInfo skillInfo)
    {
        caster.MapInstance.Broadcast(caster.GenerateCastNonTargetPacket(skillInfo));
    }

    public static void BroadcastNonTargetSkill(this IBattleEntity caster, Position position, SkillInfo skill)
    {
        caster.MapInstance.Broadcast(caster.GenerateNonTargetSkill(position, skill));
    }

    public static string GenerateNonTargetSkill(this IBattleEntity caster, Position position, SkillInfo skill) =>
        $"bs 1 {caster.Id} {position.X} {position.Y} {skill.Vnum} {skill.Cooldown} {skill.HitAnimation} {skill.HitEffect} 0 0 1 1 0 0 0";

    public static string GenerateCastNonTargetPacket(this IBattleEntity caster, SkillInfo skill) => $"ct_n 1 {caster.Id} 0 -1 {skill.CastAnimation} {skill.CastEffect} {skill.Vnum}";

    public static void BroadcastSuPacket(this IBattleEntity caster, IBattleEntity target, SkillInfo skill, int damages, SuPacketHitMode hitMode, bool isReflection = false, bool isFirst = false)
    {
        if (skill.AttackType == AttackType.Dash)
        {
            caster.MapInstance.Broadcast(caster.GenerateSuDashPacket(target, skill, damages, hitMode, isReflection, isFirst));
            return;
        }

        caster.MapInstance.Broadcast(caster.GenerateSuPacket(target, skill, damages, hitMode, target.Position, isReflection, isFirst));
    }
    
    public static void BroadCastNoEntitySuPacket(this IBattleEntity caster, IBattleEntity target, int damages,
        SuPacketHitMode hitMode, EffectType effectType)
    {
        caster.MapInstance.Broadcast(caster.GenerateNoEntitySuPacket(target, damages, hitMode, effectType));
    }

    public static void BroadcastCleanSuPacket(this IBattleEntity caster, IBattleEntity target, int damage, bool isReflect = false) => caster.MapInstance.Broadcast(caster.GenerateCleanSuPacket(target, damage, isReflect));

    public static bool CharacterCanCastOrCancel(this IBattleEntity entity, CharacterSkill skill, 
        IGameLanguageService gameLanguage, SkillInfo skillInfo, bool removeAmmo, IFishManager fishManager, IRandomGenerator randomGenerator)
    {
        var character = (IPlayerEntity)entity;

        if (character.Mp < skillInfo.ManaCost)
        {
            character.Session.SendCancelPacket(CancelType.NotInCombatMode);
            character.Session.SendSound(SoundType.NO_MANA);
            character.Session.SendChatMessage(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_ENOUGH_MP, character.Session.UserLanguage), ChatMessageColorType.Yellow);
            return false;
        }

        if (!character.WeaponLoaded(skill, gameLanguage, removeAmmo))
        {
            character.Session.SendDebugMessage("[BATTLE_DEBUG] No Weapon loaded");
            character.Session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }
        
        if (!character.CanFish(skill, fishManager, randomGenerator))
        {
            character.Session.SendDebugMessage("[BATTLE_DEBUG] can't fish here");
            character.Session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }

        if (!character.CanCollectFish(skill))
        {
            character.Session.SendDebugMessage("[BATTLE_DEBUG] can't fish here");
            character.Session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }

        if (character.SkillCanBeUsed(skill))
        {
            return true;
        }

        character.Session.SendDebugMessage("[BATTLE_DEBUG] Skill under cooldown");
        character.Session.SendCancelPacket(CancelType.NotInCombatMode);
        return false;
    }

    public static bool CharacterCanCastPartnerSkill(this IBattleEntity entity, IBattleEntitySkill skill, SkillInfo skillInfo)
    {
        var mateEntity = (IMateEntity)entity;
        return mateEntity.Mp >= skill.Skill.MpCost && mateEntity.SkillCanBeUsed(skill, DateTime.UtcNow);
    }

    public static void SendTargetConstBuffEffect(this IClientSession session, IBattleEntity battleEntity, Buff buff, int time)
    {
        session.SendPacket(battleEntity.GenerateConstBuffEffect(buff, time));
    }

    public static void SendTargetConstBuffEffects(this IClientSession session, IBattleEntity battleEntity)
    {
        session.SendPackets(battleEntity.GenerateConstBuffEffects());
    }

    public static void BroadcastConstBuffEffect(this IBattleEntity battleEntity, Buff buff, int time)
    {
        battleEntity.MapInstance?.Broadcast(battleEntity.GenerateConstBuffEffect(buff, time));
    }

    public static void BroadcastConstBuffEffects(this IBattleEntity battleEntity)
    {
        battleEntity.MapInstance?.Broadcast(battleEntity.GenerateConstBuffEffects());
    }

    public static IEnumerable<string> GenerateConstBuffEffects(this IBattleEntity battleEntity)
    {
        return battleEntity.BuffComponent.GetAllBuffs(b => b.IsConstEffect).Select(buff => battleEntity.GenerateConstBuffEffect(buff, (int)buff.Duration.TotalMilliseconds));
    }

    public static string GenerateConstBuffEffect(this IBattleEntity battleEntity, Buff buff, int time) =>
        $"bf_e {((byte)battleEntity.Type).ToString()} {battleEntity.Id.ToString()} {buff.CardId.ToString()} {time.ToString()}";

    public static async Task RemoveNegativeBuffs(this IBattleEntity entity, int level)
    {
        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => x.Level <= level && x.BuffGroup == BuffGroup.Bad && x.IsNormal());
        await entity.RemoveBuffAsync(false, buffs.ToArray());
    }
    
    public static async Task RemoveRandomNegativeBuff(this IBattleEntity entity, int level)
    {
        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => x.Level <= level && x.BuffGroup == BuffGroup.Bad && x.IsNormal());

        if (buffs.Count > 0)
        {
            int randomIndex = StaticRandomGenerator.Instance.RandomNumber(0, buffs.Count);
            Buff randomBuff = buffs[randomIndex];

            await entity.RemoveBuffAsync(false, randomBuff);
        }
    }

    public static async Task RemovePositiveBuffs(this IBattleEntity entity, int level)
    {
        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => x.Level <= level && x.BuffGroup == BuffGroup.Good && x.IsNormal());
        await entity.RemoveBuffAsync(false, buffs.ToArray());
    }

    public static async Task RemoveNeutralBuffs(this IBattleEntity entity, int level)
    {
        IReadOnlyList<Buff> buffs = entity.BuffComponent.GetAllBuffs(x => x.Level <= level && x.BuffGroup == BuffGroup.Neutral && x.IsNormal());
        await entity.RemoveBuffAsync(false, buffs.ToArray());
    }

    public static async Task RemoveInvisibility(this IBattleEntity damager)
    {
        if (!damager.BuffComponent.HasAnyBuff())
        {
            return;
        }
        if (damager is IPlayerEntity { Invisible: false })
        {
            return;
        }

        if (!damager.BCardComponent.HasBCard(BCardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide))
        {
            return;
        }
        
        if (!damager.BCardComponent.HasBCard(BCardType.LordBerios, (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence))
        {
            return;
        }

        if (damager is not IPlayerEntity charDamager)
        {
            return;
        }

        IReadOnlyList<Buff> buffs = charDamager.BuffComponent.GetAllBuffs(b => b.BCards.Any(t => (t.Type == (short)BCardType.SpecialActions && t.SubType == (byte)AdditionalTypes.SpecialActions.Hide)|| (t.Type == (short)BCardType.LordBerios &&
            t.SubType == (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence)));
        foreach (Buff buff in buffs)
        {
            await damager.RemoveBuffAsync(false, buff);
            charDamager.Session.UpdateVisibility();
        }

        foreach (IMateEntity mateEntity in charDamager.MateComponent.TeamMembers())
        {
            if (!mateEntity.BuffComponent.HasAnyBuff())
            {
                continue;
            }

            IReadOnlyList<Buff> mateBuffs =
                mateEntity.BuffComponent.GetAllBuffs(b => b.BCards.Any(t => (t.Type == (short)BCardType.SpecialActions && t.SubType == (byte)AdditionalTypes.SpecialActions.Hide) || (t.Type == (short)BCardType.LordBerios && t.SubType ==
                    (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence)));
            foreach (Buff mateBuff in mateBuffs)
            {
                await mateEntity.RemoveBuffAsync(false, mateBuff);
            }
        }
    }

    public static bool CanPerformMove(this IBattleEntity entity)
    {
        switch (entity)
        {
            case IPlayerEntity playerEntity:
                RainbowBattleParty rainbowBattleParty = playerEntity.RainbowBattleComponent.RainbowBattleParty;
                if (rainbowBattleParty != null)
                {
                    if (!playerEntity.RainbowBattleComponent.RainbowBattleParty.Started)
                    {
                        return false;
                    }

                    if (playerEntity.RainbowBattleComponent.IsFrozen)
                    {
                        return false;
                    }
                }

                if (playerEntity.IsCooking)
                {
                    return false;
                }
                
                break;

            case IMateEntity mateEntity:
                switch (mateEntity.MapInstance)
                {
                    case { MapInstanceType: MapInstanceType.RainbowBattle }:
                    case { MapInstanceType: MapInstanceType.Icebreaker }:
                        return false;
                }

                break;
        }

        return !entity.BCardComponent.HasBCard(BCardType.Move, (byte)AdditionalTypes.Move.MovementImpossible)
            && !entity.BuffComponent.HasBuff((int)BuffVnums.ETERNAL_ICE) && entity.IsAlive();
    }

    public static bool CanPerformAttack(this IBattleEntity entity)
    {
        if (entity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
        {
            return false;
        }
        
        if (entity.IsFrozenByGlacerus())
        {
            return false;
        }

        if (!entity.IsAlive())
        {
            return false;
        }

        switch (entity)
        {
            case IPlayerEntity playerEntity:
                if (playerEntity.IsSeal)
                {
                    return false;
                }

                if (playerEntity.RainbowBattleComponent.IsFrozen)
                {
                    return false;
                }
                
                if (playerEntity.BlockAllAttack)
                {
                    return false;
                }
                break;
            case IMonsterEntity mapMonster:
                switch (mapMonster.AttackType)
                {
                    case AttackType.Melee:
                        if (mapMonster.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MeleeDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Ranged:
                        if (mapMonster.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.RangedDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Magical:
                        if (mapMonster.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MagicDisabled))
                        {
                            return false;
                        }

                        break;
                }

                break;
            case IMateEntity mateEntity:
                switch (mateEntity.MapInstance)
                {
                    case { MapInstanceType: MapInstanceType.RainbowBattle }:
                    case { MapInstanceType: MapInstanceType.Icebreaker }:
                        return false;
                }

                switch (mateEntity.AttackType)
                {
                    case AttackType.Melee:
                        if (mateEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MeleeDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Ranged:
                        if (mateEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.RangedDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Magical:
                        if (mateEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MagicDisabled))
                        {
                            return false;
                        }

                        break;
                }

                break;
            case INpcEntity mapNpc:
                switch (mapNpc.AttackType)
                {
                    case AttackType.Melee:
                        if (mapNpc.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MeleeDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Ranged:
                        if (mapNpc.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.RangedDisabled))
                        {
                            return false;
                        }

                        break;
                    case AttackType.Magical:
                        if (mapNpc.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MagicDisabled))
                        {
                            return false;
                        }

                        break;
                }

                break;
        }

        return true;
    }

    public static bool CanPerformAttack(this IPlayerEntity playerEntity, SkillInfo skillInfo)
    {
        return skillInfo.AttackType switch
        {
            AttackType.Melee => !playerEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MeleeDisabled),
            AttackType.Ranged => !playerEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.RangedDisabled),
            AttackType.Magical => !playerEntity.BCardComponent.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.MagicDisabled),
            _ => true
        };
    }

    public static void ApplyAttackBCard(this IBattleEntity attacker, IBattleEntity defender, SkillInfo skillInfo, IBCardEffectHandlerContainer bCardHandler)
    {
        EquipmentType eqType = skillInfo.IsUsingSecondWeapon ? EquipmentType.MainWeapon : EquipmentType.SecondaryWeapon;
        bool isPlayer = attacker.IsPlayer();

        if (isPlayer)
        {
            IReadOnlyList<BCardDTO> shellBCards = attacker.BCardComponent.GetShellTriggers(!skillInfo.IsUsingSecondWeapon);
            foreach (BCardDTO shellBCard in shellBCards)
            {
                bCardHandler.Execute(defender, attacker, shellBCard);
            }

            byte executeMainOrSecond = skillInfo.IsUsingSecondWeapon
                ? (byte)AdditionalTypes.SpecialEffects2.SecondaryWeaponCausingChance
                : (byte)AdditionalTypes.SpecialEffects2.MainWeaponCausingChance;

            foreach ((int _, BCardDTO bCard) in attacker.BCardComponent.GetBuffBCards(x => x.Item2.Type == (short)BCardType.SpecialEffects2 && x.Item2.SubType == executeMainOrSecond))
            {
                bCardHandler.Execute(defender, attacker, bCard);
            }

            IReadOnlyDictionary<EquipmentType, List<BCardDTO>> dictionary = attacker.BCardComponent.GetEquipmentBCards();

            foreach (KeyValuePair<EquipmentType, List<BCardDTO>> bCardDictionary in dictionary)
            {
                EquipmentType equipmentType = bCardDictionary.Key;
                if (equipmentType == eqType)
                {
                    continue;
                }

                if (equipmentType != EquipmentType.MainWeapon && equipmentType != EquipmentType.SecondaryWeapon && 
                    equipmentType != EquipmentType.WeaponSkin && equipmentType != EquipmentType.CostumeSuit &&
                    equipmentType != EquipmentType.CostumeHat && equipmentType != EquipmentType.Hat &&
                    equipmentType != EquipmentType.Boots && equipmentType != EquipmentType.Gloves)
                {
                    continue;
                }

                if (bCardDictionary.Value == null)
                {
                    continue;
                }

                foreach (BCardDTO bCard in bCardDictionary.Value)
                {
                    var castType = (CastType)bCard.CastType;
                    switch (castType)
                    {
                        case CastType.ATTACK:
                            bCardHandler.Execute(defender, attacker, bCard, skillInfo);
                            break;
                        case CastType.SELF:
                        case CastType.DEFENSE:
                            bCardHandler.Execute(attacker, attacker, bCard, skillInfo);
                            break;
                    }
                }
            }

            IReadOnlyList<BCardDTO> playerAttackBCards = attacker.BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK);

            if (!playerAttackBCards.Any())
            {
                return;
            }
            
            foreach (BCardDTO bCard in playerAttackBCards)
            {
                bCardHandler.Execute(defender, attacker, bCard, skillInfo);
            }
            
            return;
        }

        if (attacker.IsMate())
        {
            IReadOnlyDictionary<EquipmentType, List<BCardDTO>> mateEq = attacker.BCardComponent.GetEquipmentBCards();

            foreach (KeyValuePair<EquipmentType, List<BCardDTO>> bCardDictionary in mateEq)
            {
                EquipmentType equipmentType = bCardDictionary.Key;
                if (equipmentType == eqType)
                {
                    continue;
                }

                if (equipmentType != EquipmentType.MainWeapon && equipmentType != EquipmentType.SecondaryWeapon && 
                    equipmentType != EquipmentType.WeaponSkin && equipmentType != EquipmentType.CostumeSuit &&
                    equipmentType != EquipmentType.CostumeHat &&
                    equipmentType != EquipmentType.Boots && equipmentType != EquipmentType.Gloves)
                {
                    continue;
                }

                if (bCardDictionary.Value == null)
                {
                    continue;
                }

                foreach (BCardDTO bCard in bCardDictionary.Value)
                {
                    var castType = (CastType)bCard.CastType;
                    switch (castType)
                    {
                        case CastType.ATTACK:
                            bCardHandler.Execute(defender, attacker, bCard);
                            break;
                        case CastType.SELF:
                        case CastType.DEFENSE:
                            bCardHandler.Execute(attacker, attacker, bCard);
                            break;
                    }
                }
            }
        }

        IReadOnlyList<BCardDTO> attackerBCards = attacker.BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK);

        if (!attackerBCards.Any())
        {
            return;
        }

        if (attacker.BCardComponent.HasBCard(BCardType.Mode, (byte)AdditionalTypes.Mode.DirectDamage))
        {
            return;
        }

        foreach (BCardDTO bCard in attackerBCards.ToList())
        {
            switch ((CastType)bCard.CastType)
            {
                case CastType.ATTACK: // during attacking on enemy (the target)
                    bCardHandler.Execute(defender, attacker, bCard, skillInfo);
                    break;
                case CastType.DEFENSE: // during attacking on self
                    bCardHandler.Execute(attacker, attacker, bCard, skillInfo);
                    break;
                default:
                    continue;
            }
        }
    }

    public static void ApplyDefenderBCard(this IBattleEntity defender, IBattleEntity attacker, SkillInfo skillInfo, IBCardEffectHandlerContainer bCardHandler)
    {
        bool isPlayer = defender.IsPlayer();

        if (isPlayer)
        {
            IReadOnlyDictionary<EquipmentType, List<BCardDTO>> dictionary = defender.BCardComponent.GetEquipmentBCards();

            foreach ((EquipmentType equipmentType, List<BCardDTO> value) in dictionary)
            {
                if (equipmentType != EquipmentType.MainWeapon && equipmentType != EquipmentType.SecondaryWeapon && 
                    equipmentType != EquipmentType.WeaponSkin && equipmentType != EquipmentType.CostumeSuit &&
                    equipmentType != EquipmentType.CostumeHat && equipmentType != EquipmentType.Hat &&
                    equipmentType != EquipmentType.Boots && equipmentType != EquipmentType.Gloves)
                {
                    continue;
                }

                if (value == null)
                {
                    continue;
                }

                foreach (BCardDTO bCard in value)
                {
                    var castType = (CastType)bCard.CastType;
                    switch (castType)
                    {
                        case CastType.ATTACK:
                            bCardHandler.Execute(attacker, defender, bCard, skillInfo);
                            break;
                        case CastType.SELF:
                        case CastType.DEFENSE:
                            bCardHandler.Execute(defender, defender, bCard, skillInfo);
                            break;
                    }
                }
            }
            
            IReadOnlyList<BCardDTO> playerDefenceBCards = defender.BCardComponent.GetTriggerBCards(BCardTriggerType.DEFENSE);

            if (!playerDefenceBCards.Any())
            {
                return;
            }
            
            foreach (BCardDTO bCard in playerDefenceBCards)
            {
                bCardHandler.Execute(attacker, defender, bCard, skillInfo);
            }

            return;
        }
        
        if (defender.IsMate())
        {
            IReadOnlyDictionary<EquipmentType, List<BCardDTO>> mateEq = attacker.BCardComponent.GetEquipmentBCards();

            foreach (KeyValuePair<EquipmentType, List<BCardDTO>> bCardDictionary in mateEq)
            {
                EquipmentType equipmentType = bCardDictionary.Key;

                if (equipmentType != EquipmentType.MainWeapon && equipmentType != EquipmentType.SecondaryWeapon && 
                    equipmentType != EquipmentType.WeaponSkin && equipmentType != EquipmentType.CostumeSuit &&
                    equipmentType != EquipmentType.CostumeHat &&
                    equipmentType != EquipmentType.Boots && equipmentType != EquipmentType.Gloves)
                {
                    continue;
                }

                if (bCardDictionary.Value == null)
                {
                    continue;
                }

                foreach (BCardDTO bCard in bCardDictionary.Value)
                {
                    var castType = (CastType)bCard.CastType;
                    switch (castType)
                    {
                        case CastType.ATTACK:
                            bCardHandler.Execute(defender, attacker, bCard);
                            break;
                        case CastType.SELF:
                        case CastType.DEFENSE:
                            bCardHandler.Execute(attacker, attacker, bCard);
                            break;
                    }
                }
            }
        }

        IReadOnlyList<BCardDTO> defenderBCards = defender.BCardComponent.GetTriggerBCards(BCardTriggerType.DEFENSE);

        if (!defenderBCards.Any())
        {
            return;
        }

        foreach (BCardDTO bCard in defenderBCards.ToList())
        {
            switch ((CastType)bCard.CastType)
            {
                case CastType.ATTACK: // on getting hit on enemy (the attacker)
                    bCardHandler.Execute(attacker, defender, bCard, skillInfo);
                    break;
                case CastType.DEFENSE: // on getting hit on self
                    bCardHandler.Execute(defender, defender, bCard, skillInfo);
                    break;
            }
        }
    }


    public static void InitializeBCards(this IBattleEntity entity)
    {
        var attackBCards = new List<BCardDTO>();
        var defenseBCards = new List<BCardDTO>();

        IReadOnlyList<BCardDTO> entityBCards = entity switch
        {
            IMonsterEntity mapMonster => mapMonster.BCards,
            IMateEntity mateEntity => mateEntity.BCards,
            INpcEntity mapNpc => mapNpc.BCards,
            _ => null
        };

        if (entityBCards == null)
        {
            Log.Warn($"Couldn't initialize BCards of this BattleEntity -> VisualType: '{entity.Type.ToString()}' Id: '{entity.Id.ToString()}'");
            return;
        }

        foreach (BCardDTO bCard in entityBCards)
        {
            switch (bCard.NpcTriggerType)
            {
                case BCardNpcTriggerType.ON_ATTACK:
                    if ((CastType)bCard.CastType == CastType.SELF)
                    {
                        entity.BCardComponent.AddBCard(bCard);
                    }
                    else
                    {
                        attackBCards.Add(bCard);
                    }

                    break;
                case BCardNpcTriggerType.ON_DEFENSE:
                    if ((CastType)bCard.CastType == CastType.SELF)
                    {
                        entity.BCardComponent.AddBCard(bCard);
                    }
                    else
                    {
                        defenseBCards.Add(bCard);
                    }

                    break;
                default:
                    entity.BCardComponent.AddBCard(bCard);
                    break;
            }
        }

        entity.BCardComponent.AddTriggerBCards(BCardTriggerType.ATTACK, attackBCards);
        entity.BCardComponent.AddTriggerBCards(BCardTriggerType.DEFENSE, defenseBCards);

        if (entity is not IMonsterEntity monsterEntity)
        {
            return;
        }

        monsterEntity.RefreshStats();
    }

    public static bool IsSameEntity(this IBattleEntity entity, IBattleEntity target) => entity != null && target != null && entity.Id == target.Id && entity.Type == target.Type;

    public static void SetSkillCooldown(this IBattleEntity caster, SkillInfo skill)
    {
        short cooldownReduction = skill.Cooldown;
        List<IBattleEntitySkill> skills = caster.Skills;
        IBattleEntitySkill battleEntitySkill;

        if (caster.IsMonster() || caster.IsNpc())
        {
            battleEntitySkill = skills.FirstOrDefault(x => x.Skill.Id == skill.Vnum);
        }
        else
        {
            battleEntitySkill = caster.IsPlayer() ? skills.FirstOrDefault(x => x.Skill.CastId == skill.CastId && x.Skill.SkillType == SkillType.NormalPlayerSkill) : skills.FirstOrDefault(x => x.Skill.CastId == skill.CastId);
        }

        if (battleEntitySkill == null)
        {
            return;
        }

        DateTime time = DateTime.UtcNow.AddMilliseconds(cooldownReduction * 100);
        battleEntitySkill.LastUse = time;

        switch (caster)
        {
            case IPlayerEntity character:
                character.AddSkillCooldown(time, skill.CastId);
                break;
            case IMateEntity mateEntity:
                if (skill.Vnum == 0)
                {
                    return;
                }

                mateEntity.Owner?.AddMateSkillCooldown(time, skill.CastId, mateEntity.MateType);
                break;
        }
    }

    public static string GenerateDiePacket(this IBattleEntity entity) => $"die {(byte)entity.Type} {entity.Id} {(byte)entity.Type} {entity.Id}";
    public static void BroadcastDie(this IBattleEntity entity) => entity.MapInstance.Broadcast(entity.GenerateDiePacket());
    public static string GeneratePushPacket(this IBattleEntity entity, short x, short y, int value) => $"guri 3 {(byte)entity.Type} {entity.Id} {x} {y} 3 {value} 2 -1 0";

    public static string GenerateDashGuriPacket(this IBattleEntity entity, short x, short y, int value)
        => $"guri 3 {(byte)entity.Type} {entity.Id} {x} {y} 3 2 2 {value}";

    public static async Task RemoveSacrifice(this IBattleEntity caster, IBattleEntity target, ISacrificeManager sacrificeManager, IGameLanguageService gameLanguage)
    {
        Buff sacrifice = caster.BuffComponent.GetBuff((short)BuffVnums.SPIRIT_OF_SACRIFICE);
        await caster.RemoveBuffAsync(false, sacrifice);
        Buff nobleGesture = target.BuffComponent.GetBuff((short)BuffVnums.NOBLE_GESTURE);
        await target.RemoveBuffAsync(false, nobleGesture);
        string message;
        if (caster is IPlayerEntity character)
        {
            message = gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_REMOVED, character.Session.UserLanguage);
            character.Session.SendMsg(message, MsgMessageType.SmallMiddle);
        }

        if (target is IPlayerEntity targetCharacter)
        {
            message = gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_REMOVED, targetCharacter.Session.UserLanguage);
            targetCharacter.Session.SendMsg(message, MsgMessageType.SmallMiddle);
        }
    }

    public static void RemoveBuddha(this IClientSession session)
    {
        if (!session.PlayerEntity.SkillComponent.BuddhaWordsActivated)
        {
            return;
        }

        SkillInfo fakeBuddha = session.PlayerEntity.GetFakeBuddhaSkill();
        session.SendSkillCooldownResetAfter(fakeBuddha.CastId, fakeBuddha.Cooldown);
        session.PlayerEntity.SetSkillCooldown(fakeBuddha);
        session.PlayerEntity.SkillComponent.BuddhaWordsActivated = false;
    }

    public static bool CanBeInterrupted(this IPlayerEntity character, SkillInfo skillInfo)
    {
        if (character.Class != ClassType.Magician && character.Class != ClassType.Adventurer)
        {
            return false;
        }

        if (character.GetMaxWeaponShellValue(ShellEffectType.AntiMagicDisorder) != 0)
        {
            return false;
        }

        int undisturbedWhileCasting = character.BCardComponent.GetAllBCardsInformation(BCardType.AntiMagicDisorder, (byte)AdditionalTypes.AntiMagicDisorder.UndisturbedWhileCasting, character.Level).firstData;

        if (undisturbedWhileCasting != 0)
        {
            return false;
        }

        if (skillInfo.AttackType != AttackType.Magical || skillInfo.IsUsingSecondWeapon)
        {
            return false;
        }

        return !character.SkillComponent.IsSkillInterrupted;
    }

    public static string GenerateShadowFigurePacket(this IBattleEntity entity, int firstValue, int secondValue) => $"guri 0 {(byte)entity.Type} {entity.Id} {firstValue} {secondValue}";

    public static void BroadcastShadowFigurePacket(this IBattleEntity entity, int firstValue, int secondValue) =>
        entity.MapInstance?.Broadcast(entity.GenerateShadowFigurePacket(firstValue, secondValue));

    public static void ShadowAppears(this IBattleEntity battleEntity, bool broadcastZeroValue, Buff buff = null)
    {
        if (buff != null && broadcastZeroValue && buff.BCards.Any(x => x.Type == (short)BCardType.SpecialEffects && x.SubType == (byte)AdditionalTypes.SpecialEffects.ShadowAppears))
        {
            battleEntity.BroadcastShadowFigurePacket(0, 0);
        }

        if (!battleEntity.BuffComponent.HasAnyBuff())
        {
            return;
        }

        if (!battleEntity.BCardComponent.HasBCard(BCardType.SpecialEffects, (byte)AdditionalTypes.SpecialEffects.ShadowAppears))
        {
            battleEntity.BroadcastShadowFigurePacket(0, 0);
            return;
        }

        (int firstData, int secondData) = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.SpecialEffects,
            (byte)AdditionalTypes.SpecialEffects.ShadowAppears, battleEntity.Level);
        if (firstData == 0 && secondData == 0)
        {
            battleEntity.BroadcastShadowFigurePacket(0, 0);
            return;
        }

        battleEntity.BroadcastShadowFigurePacket(!broadcastZeroValue ? firstData : 0, secondData);
    }

    public static DateTime GenerateSkillCastTime(this IBattleEntity entity, SkillInfo skillInfo, bool isPartnerSkill = false)
    {
        DateTime time = DateTime.UtcNow.AddMilliseconds(GenerateSkillCastTimeNumber(entity, skillInfo, isPartnerSkill));
        return time;
    }

    public static short GenerateSkillCastTimeNumber(this IBattleEntity entity, SkillInfo skillInfo, bool isPartnerSkill = false)
    {
        short milliSeconds = (short)(skillInfo.CastTime * 100);

        int toIncrease = entity.BCardComponent.GetAllBCardsInformation(BCardType.Casting, (byte)AdditionalTypes.Casting.EffectDurationIncreased, entity.Level).firstData;
        int toDecrease = entity.BCardComponent.GetAllBCardsInformation(BCardType.Casting, (byte)AdditionalTypes.Casting.EffectDurationDecreased, entity.Level).firstData;

        int absoluteIncrease = skillInfo.AttackType switch
        {
            AttackType.Melee => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.MeleeDurationIncreased, entity.Level).firstData,
            AttackType.Ranged => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.RangedDurationIncreased, entity.Level).firstData,
            AttackType.Magical => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.MagicalDurationIncreased, entity.Level).firstData,
            _ => 0
        };

        int absoluteDecrease = skillInfo.AttackType switch
        {
            AttackType.Melee => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.MeleeDurationDecreased, entity.Level).firstData,
            AttackType.Ranged => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.RangedDurationDecreased, entity.Level).firstData,
            AttackType.Magical => entity.BCardComponent.GetAllBCardsInformation(BCardType.JumpBackPush, (byte)AdditionalTypes.JumpBackPush.MagicalDurationDecreased, entity.Level).firstData,
            _ => 0
        };

        int toAddMilliSeconds = milliSeconds;

        toAddMilliSeconds = (int)(toAddMilliSeconds * (1 + toIncrease * 0.01));
        toAddMilliSeconds = (int)(toAddMilliSeconds * (1 - toDecrease * 0.01));

        toAddMilliSeconds += absoluteIncrease;
        toAddMilliSeconds -= absoluteDecrease;

        milliSeconds = (short)toAddMilliSeconds;

        if (isPartnerSkill)
        {
            milliSeconds = (short)((milliSeconds - 1) * 2);
        }

        short finalCooldown = milliSeconds;

        return finalCooldown;
    }

    public static int CalculateManaUsage(this IBattleEntity entity, int mana, SkillDTO skill = null)
    {
        double manaMultiplier = 1;

        if (skill is { AttackType: AttackType.Magical })
        {
            manaMultiplier -= entity.BCardComponent.GetAllBCardsInformation(BCardType.Casting,
                (byte)AdditionalTypes.Casting.ManaForSkillsDecreased, entity.Level).firstData * 0.01;

            manaMultiplier += entity.BCardComponent.GetAllBCardsInformation(BCardType.Casting,
                (byte)AdditionalTypes.Casting.ManaForSkillsIncreased, entity.Level).firstData * 0.01;
        }

        if (entity is IPlayerEntity playerEntity)
        {
            manaMultiplier -= playerEntity.GetJewelsCellonsValue(CellonType.MpConsumption) * 0.01;
            manaMultiplier -= (playerEntity.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.REDUCE_MP_CONSUMPTION).Item1 ?? 0) * 0.01;
            manaMultiplier -= playerEntity.GetMaxWeaponShellValue(ShellEffectType.ReducedMPConsume) * 0.01;
        }

        return (int)(mana * manaMultiplier);
    }

    public static void RemoveEntityMp(this IBattleEntity battleEntity, short mana, SkillDTO skill = null)
    {
        int mp = battleEntity.CalculateManaUsage(mana, skill);

        battleEntity.Mp = battleEntity.Mp - mp < 0 ? 0 : battleEntity.Mp - mp;
    }

    public static bool IsMateTrainer(this IBattleEntity entity) => entity is IMonsterEntity { IsMateTrainer: true };
    
    public static bool IsSparringMonster(this IBattleEntity entity) => entity is IMonsterEntity { IsSparringMonster: true };

    public static bool IsInvisibleGm(this IBattleEntity entity) => entity is IPlayerEntity player && player.CheatComponent.IsInvisible;
    
    public static bool IsSucceededChance(this IBattleEntity entity, int chance) => _randomGenerator.RandomNumber() <= chance;
}