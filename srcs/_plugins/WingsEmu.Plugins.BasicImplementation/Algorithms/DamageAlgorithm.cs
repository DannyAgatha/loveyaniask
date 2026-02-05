// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Helpers.Damages.Calculation;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public class DamageAlgorithm : IDamageAlgorithm
{
    private readonly IBuffFactory _buffFactory;

    private readonly HashSet<int> _specialSkills = new()
    {
        (int)SkillsVnums.GIANT_SWIRL,
        (int)SkillsVnums.FOG_ARROW,
        (int)SkillsVnums.FIRE_MINE,
        (int)SkillsVnums.BOMB
    };

    public DamageAlgorithm(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public DamageAlgorithmResult GenerateDamage(IBattleEntityDump attacker, IBattleEntityDump defender, SkillInfo skill)
    {
        int damages = 0;
        HitType hitMode = HitType.Normal;
        bool dragonEffect = false;
        
        if (defender == null)
        {
            return new DamageAlgorithmResult(damages, hitMode, false, false);
        }

        if (skill == null)
        {
            return new DamageAlgorithmResult(damages, hitMode, false, false);
        }

        if ((!skill.BCards.Any(x => x.Type == (short)BCardType.LordHatus && x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf) &&
                skill.TargetAffectedEntities != TargetAffectedEntities.Enemies) || _specialSkills.Contains(skill.Vnum))
        {
            return new DamageAlgorithmResult(damages, hitMode, false, false);
        }
        
        CalculationBasicStatistics basicCalculation = attacker.CalculateBasicStatistics(defender, skill);

        IBattleEntity targetEntity = attacker.MapInstance.GetBattleEntity(defender.Type, defender.Id);

        if (attacker.IsMiss(defender, basicCalculation, skill) && skill.PartnerSkillRank is not 7)
        {
            hitMode = HitType.Miss;
            return new DamageAlgorithmResult(damages, hitMode, false, false);
        }

        CalculationDefense defense = attacker.CalculationDefense(defender);
        CalculationPhysicalDamage physicalDamage = attacker.CalculatePhysicalDamage(defender, skill);
        CalculationElementDamage elementDamage = attacker.CalculateElementDamage(defender, skill);
        CalculationResult damageResult = attacker.CalculationResult(defender, basicCalculation, defense, physicalDamage, elementDamage, skill);

        if (damageResult.IsCritical)
        {
            hitMode = HitType.Critical;
        }

        damages = damageResult.Damage;
        
        var dragonTypes = new Dictionary<(BCardType Type, byte SubType), byte>
        {
            [(BCardType.LordCalvinas, (byte)AdditionalTypes.LordCalvinas.SpawnNeutralDragon)] = (byte)AdditionalTypes.LordCalvinas.SpawnNeutralDragon,
            [(BCardType.LordCalvinas, (byte)AdditionalTypes.LordCalvinas.SpawnFireDragon)] = (byte)AdditionalTypes.LordCalvinas.SpawnFireDragon,
            [(BCardType.LordCalvinas, (byte)AdditionalTypes.LordCalvinas.SpawnIceDragon)] = (byte)AdditionalTypes.LordCalvinas.SpawnIceDragon,
            [(BCardType.LordCalvinas, (byte)AdditionalTypes.LordCalvinas.SpawnMoonDragon)] = (byte)AdditionalTypes.LordCalvinas.SpawnMoonDragon,
            [(BCardType.SESpecialist, (byte)AdditionalTypes.SESpecialist.SpawnSkyDragon)] = (byte)AdditionalTypes.SESpecialist.SpawnSkyDragon
        };
        
        foreach (KeyValuePair<(BCardType Type, byte SubType), byte> dragonType in dragonTypes)
        {
            (int firstData, _, _) = attacker.GetBCardInformation(dragonType.Key.Type, dragonType.Value);
            
            if (!attacker.IsSucceededChance(firstData))
            {
                continue;
            }

            dragonEffect = true;
            break;
        }

        if (defender.GetBCardInformation(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower).count == 0)
        {
            return new DamageAlgorithmResult(damages, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }
        
        if (targetEntity == null)
        {
            return new DamageAlgorithmResult(damages, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }
        
        if (defender.HasBuff((int)BuffVnums.WEAK_BERSERK_SPIRIT))
        {
            targetEntity.RemoveBuffAsync((int)BuffVnums.WEAK_BERSERK_SPIRIT).ConfigureAwait(false).GetAwaiter().GetResult();
            targetEntity.AddBuffAsync(_buffFactory.CreateBuff((int)BuffVnums.BERSERK_SPIRIT, targetEntity)).ConfigureAwait(false).GetAwaiter().GetResult();
            return new DamageAlgorithmResult(0, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }

        if (defender.HasBuff((int)BuffVnums.BERSERK_SPIRIT))
        {
            targetEntity.RemoveBuffAsync((int)BuffVnums.BERSERK_SPIRIT).ConfigureAwait(false).GetAwaiter().GetResult();
            targetEntity.AddBuffAsync(_buffFactory.CreateBuff((int)BuffVnums.STRONG_BERSERK_SPIRIT, targetEntity)).ConfigureAwait(false).GetAwaiter().GetResult();
            return new DamageAlgorithmResult(0, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }

        if (defender.HasBuff((int)BuffVnums.LICH_MAGIC))
        {
            targetEntity.RemoveBuffAsync((int)BuffVnums.LICH_MAGIC).ConfigureAwait(false).GetAwaiter().GetResult();
            return new DamageAlgorithmResult(0, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }

        if (defender.HasBuff((int)BuffVnums.STRONG_LICH_MAGIC))
        {
            targetEntity.RemoveBuffAsync((int)BuffVnums.STRONG_LICH_MAGIC).ConfigureAwait(false).GetAwaiter().GetResult();
            return new DamageAlgorithmResult(0, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }
        
        if (defender.GetBCardInformation(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower).count == 0)
        {
            return new DamageAlgorithmResult(damages, hitMode, damageResult.IsSoftDamage, dragonEffect);
        }

        if (targetEntity.ChargeComponent.GetCharge() != 0)
        {
            return new DamageAlgorithmResult(0, HitType.Miss, false, dragonEffect);
        }
        
        targetEntity.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.IMPROVED_CHARGE, targetEntity)).ConfigureAwait(false).GetAwaiter().GetResult();
        return new DamageAlgorithmResult(0, HitType.Miss, false, dragonEffect);
    }
}