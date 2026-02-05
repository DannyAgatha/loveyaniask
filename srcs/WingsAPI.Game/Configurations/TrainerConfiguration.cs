using System;
using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Configurations;

public interface ITrainerConfiguration
{
    IReadOnlyList<TrainerConfigElement> GetAttackConfigs(int level);
    IReadOnlyList<TrainerConfigElement> GetDefenseConfigs(int level);
    bool IsStrongTrainer(int monsterVnum);
    bool CanLoseAttack(int monsterVnum);
    bool CanLoseDefense(int monsterVnum);
}

public class TrainerConfiguration : ITrainerConfiguration
{
    private readonly IReadOnlyDictionary<int, List<TrainerConfigElement>> _attack;
    private readonly IReadOnlyDictionary<int, List<TrainerConfigElement>> _defense;
    private readonly IEnumerable<int> _strongTrainers;
    private readonly IEnumerable<int> _cantLoseAttack;
    private readonly IEnumerable<int> _cantLoseDefense;

    public TrainerConfiguration(TrainerConfig configuration)
    {
        Dictionary<int, List<TrainerConfigElement>> attackConfig = new();
        Dictionary<int, List<TrainerConfigElement>> defenseConfig = new();

        foreach (TrainerConfigElement trainerAttackConfig in configuration.AttackUpgradeConfig)
        {
            if (!attackConfig.TryGetValue(trainerAttackConfig.FromLevel, out List<TrainerConfigElement> list))
            {
                list = new();
                attackConfig[trainerAttackConfig.FromLevel] = list;
            }

            list.Add(trainerAttackConfig);
        }

        foreach (TrainerConfigElement trainerDefenseConfig in configuration.DefenseUpgradeConfig)
        {
            if (!defenseConfig.TryGetValue(trainerDefenseConfig.FromLevel, out List<TrainerConfigElement> list))
            {
                list = new();
                defenseConfig[trainerDefenseConfig.FromLevel] = list;
            }

            list.Add(trainerDefenseConfig);
        }

        _attack = attackConfig;
        _defense = defenseConfig;
        _strongTrainers = configuration.StrongTrainers;
        _cantLoseAttack = configuration.SpecialTrainersCantLoseAttack;
        _cantLoseDefense = configuration.SpecialTrainersCantLoseDefense;
    }

    public IReadOnlyList<TrainerConfigElement> GetAttackConfigs(int level) => _attack.TryGetValue(level, out List<TrainerConfigElement> list) ? list : Array.Empty<TrainerConfigElement>();
    public IReadOnlyList<TrainerConfigElement> GetDefenseConfigs(int level) => _defense.TryGetValue(level, out List<TrainerConfigElement> list) ? list : Array.Empty<TrainerConfigElement>();
    public bool IsStrongTrainer(int monsterVnum) => _strongTrainers.Contains(monsterVnum);
    public bool CanLoseAttack(int monsterVnum) => !_cantLoseAttack.Contains(monsterVnum);
    public bool CanLoseDefense(int monsterVnum) => !_cantLoseDefense.Contains(monsterVnum);
}

public class TrainerConfig
{
    public IEnumerable<TrainerConfigElement> AttackUpgradeConfig { get; set; }
    public IEnumerable<TrainerConfigElement> DefenseUpgradeConfig { get; set; }
    public IEnumerable<int> StrongTrainers { get; set; }
    public IEnumerable<int> SpecialTrainersCantLoseAttack { get; set; }
    public IEnumerable<int> SpecialTrainersCantLoseDefense { get; set; }
}

public class TrainerConfigElement
{
    public int FromLevel { get; set; }
    public int HitsRequired { get; set; }
    public int SuccessChance { get; set; }
    public int FailChance { get; set; }
    public int NoLearningChance { get; set; }
    public bool IsStrongTrainer { get; set; }
    public bool CanLoseAttack { get; set; }
    public bool CanLoseDefense { get; set; }
}