using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.DTOs.Skills;

namespace WingsEmu.Game.Managers.StaticData;

public class StaticSkillsManager
{
    public static ISkillsManager Instance { get; private set; }

    public static void Initialize(ISkillsManager manager)
    {
        Instance = manager;
    }
}

public interface ISkillsManager
{
    Task Initialize();
    SkillDTO GetSkill(int s);
    List<SkillDTO> GetSkill(string name);
    IEnumerable<SkillDTO> GetSkills();
}