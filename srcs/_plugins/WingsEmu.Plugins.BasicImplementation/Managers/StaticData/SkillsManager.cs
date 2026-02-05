using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.Managers.StaticData;

public class SkillsManager : ISkillsManager
{
    private readonly IKeyValueCache<List<SkillDTO>> _skillByName;
    private readonly ILongKeyCachedRepository<SkillDTO> _skillCache;
    private readonly IResourceLoader<SkillDTO> _skillDao;
    private SkillDTO[] _skills;

    public SkillsManager(ILongKeyCachedRepository<SkillDTO> skillCache, IKeyValueCache<List<SkillDTO>> skillByName, IResourceLoader<SkillDTO> skillDao)
    {
        _skillDao = skillDao;
        _skillCache = skillCache;
        _skillByName = skillByName;
    }

    public async Task Initialize()
    {
        _skills = (await _skillDao.LoadAsync()).ToArray();
        int skillsLoaded = 0;
        foreach (SkillDTO skillItem in _skills)
        {
            _skillCache.Set(skillItem.Id, skillItem);
            _skillByName.GetOrSet(skillItem.Name, () => new List<SkillDTO>()).Add(skillItem);
            skillsLoaded++;
        }

        Log.Info($"[DATABASE] Loaded {skillsLoaded} skills.");
    }

    public SkillDTO GetSkill(int s) => _skillCache.Get(s);

    public List<SkillDTO> GetSkill(string s) => _skillByName.Get(s);

    public IEnumerable<SkillDTO> GetSkills() => _skills;
}