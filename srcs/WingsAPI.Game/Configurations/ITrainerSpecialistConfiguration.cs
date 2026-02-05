using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game.Configurations;

public interface ITrainerSpecialistConfiguration
{
    IReadOnlyList<int> GetMonstersBySkillVnum(int skillVnum);
    int? GetMaxMonsterPerMap(int skillVnum);
    int? GetExperienceGivenByMonsterVnum(int monsterVnum);
    int? GetMaxHeroLevelTargetByMonsterVnum(int monsterVnum);
}

public class TrainerSpecialistConfiguration : ITrainerSpecialistConfiguration
{
    private readonly Dictionary<int, TrainerSpecialist> _monsters;
    private readonly ImmutableList<TrainerSpecialist> _skills;

    public TrainerSpecialistConfiguration(TrainerSpecialistFileConfiguration monsters, TrainerSpecialistFileConfiguration skills)
    {
        _monsters = monsters.ToDictionary(s => s.SkillVnum);
        _skills = skills.ToImmutableList();
    }

    public IReadOnlyList<int> GetMonstersBySkillVnum(int skillVnum)
    {
        TrainerSpecialist tmp = _monsters.GetOrDefault(skillVnum);
        return tmp?.PossibleMonsters;
    }

    public int? GetMaxMonsterPerMap(int skillVnum) => _skills.FirstOrDefault(s => s.SkillVnum == skillVnum)?.MaxMonsterPerMap;

    public int? GetExperienceGivenByMonsterVnum(int monsterVnum)
    {
        foreach (TrainerSpecialist specialist in _monsters.Values.Where(specialist => specialist.PossibleMonsters.Contains(monsterVnum)))
        {
            return specialist.ExperienceGiven;
        }

        return null;
    }

    public int? GetMaxHeroLevelTargetByMonsterVnum(int monsterVnum)
    {
        foreach (TrainerSpecialist specialist in _monsters.Values.Where(specialist => specialist.PossibleMonsters.Contains(monsterVnum)))
        {
            return specialist.MaxTargetHeroLevel;
        }

        return null;
    }
}

public class TrainerSpecialistFileConfiguration : List<TrainerSpecialist>
{
}

public class TrainerSpecialist
{
    public int SkillVnum { get; set; }
    public List<int> PossibleMonsters { get; set; }
    public int MaxMonsterPerMap { get; set; }
    public int ExperienceGiven { get; set; }
    public int MaxTargetHeroLevel { get; set; }
}