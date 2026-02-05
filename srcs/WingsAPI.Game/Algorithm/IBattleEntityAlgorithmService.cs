using WingsEmu.Game._enum;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Algorithm;

public class StaticBattleEntityAlgorithmService
{
    public static IBattleEntityAlgorithmService Instance { get; private set; }

    public static void Initialize(IBattleEntityAlgorithmService instance)
    {
        Instance = instance;
    }
}

public interface IBattleEntityAlgorithmService
{
    /// <summary>
    ///     Returns the MaxHp based on basic algorithm
    /// </summary>
    /// <param name="race"></param>
    /// <param name="level"></param>
    /// <param name="additionalHp"></param>
    /// <param name="modifier"></param>
    /// <param name="isMonster"></param>
    /// <returns></returns>
    int GetBasicHp(int race, int level, int modifier, int additionalHp = 0, bool isMonster = true);

    /// <summary>
    ///     Returns the MaxMp based on basic algorithm
    /// </summary>
    /// <param name="race"></param>
    /// <param name="level"></param>
    /// <param name="modifier"></param>
    /// <param name="additionalMp"></param>
    /// <param name="isMonster"></param>
    /// <returns></returns>
    int GetBasicMp(int race, int level, int modifier, int additionalMp = 0, bool isMonster = true);

    /// <summary>
    ///     Returns the MaxHp based on ClassType
    /// </summary>
    /// <param name="classType"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    int GetBasicHpByClass(ClassType classType, int level);

    /// <summary>
    ///     Returns the MaxMp based on ClassType
    /// </summary>
    /// <param name="classType"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    int GetBasicMpByClass(ClassType classType, int level);

    /// <summary>
    ///     Get minimum/maximum attack damage
    /// </summary>
    /// <param name="isMin"></param>
    /// <param name="race"></param>
    /// <param name="attackType"></param>
    /// <param name="weaponLevel"></param>
    /// <param name="wInfo"></param>
    /// <param name="level"></param>
    /// <param name="modifier"></param>
    /// <param name="additional"></param>
    /// <param name="isWild"></param>
    /// <param name="petLevel"></param>
    /// <param name="mateType"></param>
    /// <returns></returns>
    int GetAttack(bool isMin, int race, AttackType attackType, short weaponLevel, byte wInfo, short level, int modifier, int additional, bool isWild = true, short petLevel = 0,
        MateType mateType = MateType.Pet);

    /// <summary>
    ///     Get hitrate
    /// </summary>
    /// <param name="race"></param>
    /// <param name="attackType"></param>
    /// <param name="weaponLevel"></param>
    /// <param name="level"></param>
    /// <param name="modifier"></param>
    /// <param name="additional"></param>
    /// <param name="isWild"></param>
    /// <param name="petLevel"></param>
    /// <param name="mateType"></param>
    /// <returns></returns>
    int GetHitrate(int race, AttackType attackType, short weaponLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Pet);

    /// <summary>
    ///     Get dodge
    /// </summary>
    /// <param name="race"></param>
    /// <param name="armorLevel"></param>
    /// <param name="level"></param>
    /// <param name="modifier"></param>
    /// <param name="additional"></param>
    /// <param name="isWild"></param>
    /// <param name="petLevel"></param>
    /// <returns></returns>
    int GetDodge(int race, short armorLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Pet);

    /// <summary>
    ///     Get defense
    /// </summary>
    /// <param name="race"></param>
    /// <param name="attackType"></param>
    /// <param name="armorLevel"></param>
    /// <param name="level"></param>
    /// <param name="modifier"></param>
    /// <param name="additional"></param>
    /// <param name="isWild"></param>
    /// <param name="petLevel"></param>
    /// <param name="mateType"></param>
    /// <returns></returns>
    int GetDefense(int race, AttackType attackType, short armorLevel, short level, int modifier, int additional, bool isWild = true, short petLevel = 0, MateType mateType = MateType.Pet);

    /// <summary>
    ///     Get Speed
    /// </summary>
    /// <returns></returns>
    byte GetSpeed(ClassType classType);
    
    int GetTrainerSpecialistExperience(byte stars, byte heroLevel);
    
    int GetStarsAttackPower(byte stars, byte heroLevel);

    int GetStarsHitRate(byte stars, byte heroLevel);

    int GetStarsAllDefences(byte stars, byte heroLevel);

    int GetStarsHpMp(byte stars, byte heroLevel);

    int GetStarsAllResistances(byte stars, byte heroLevel);

    int GetBaseStatistic(int level, ClassType classType, StatisticType statisticType);
}