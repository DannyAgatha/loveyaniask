// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.Drops;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.NpcMonster;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Managers.StaticData;

public class NpcMonsterManager : INpcMonsterManager
{
    private readonly IDropManager _dropManager;
    private readonly IEntitySkillFactory _entitySkillFactory;

    private readonly IKeyValueCache<List<IMonsterData>> _npcMonsterByName;
    private readonly ILongKeyCachedRepository<IMonsterData> _npcMonsterCache;
    private readonly IResourceLoader<NpcMonsterDto> _npcMonsterLoader;
    private readonly ISkillsManager _skillsManager;

    public NpcMonsterManager(ILongKeyCachedRepository<IMonsterData> npcMonsterCache, IKeyValueCache<List<IMonsterData>> npcMonsterByName, ISkillsManager skillsManager, IDropManager dropManager,
        IResourceLoader<NpcMonsterDto> npcMonsterLoader, IEntitySkillFactory entitySkillFactory)
    {
        _npcMonsterCache = npcMonsterCache;
        _npcMonsterByName = npcMonsterByName;
        _skillsManager = skillsManager;
        _dropManager = dropManager;
        _npcMonsterLoader = npcMonsterLoader;
        _entitySkillFactory = entitySkillFactory;
    }

    public async Task InitializeAsync()
    {
        int npcMonstersAdded = 0;

        foreach (NpcMonsterDto npcMonster in await _npcMonsterLoader.LoadAsync())
        {
            if (npcMonster.Adapt<MonsterData>() is not { } monster)
            {
                continue;
            }

            var drop = new List<DropDTO>();

            if (npcMonster.BCards.Any())
            {
                foreach (BCardDTO s in npcMonster.BCards)
                {
                    if (s.Type == (short)BCardType.SpecialActions && s.SubType == (byte)AdditionalTypes.SpecialActions.SeeHiddenThings)
                    {
                        monster.CanSeeInvisible = true;
                    }
                }
            }

            if (npcMonster.Drops != null)
            {
                drop.AddRange(npcMonster.Drops);
            }

            drop.AddRange(_dropManager.GetDropsByMonsterVnum(monster.MonsterVNum));
            monster.Drops = drop;

            _npcMonsterByName.GetOrSet(monster.Name, () => new List<IMonsterData>()).Add(monster);
            _npcMonsterCache.Set(monster.Id, monster);
            npcMonstersAdded++;
        }

        Log.Info($"[DATABASE] Loaded {npcMonstersAdded} monsters.");
    }

    public IMonsterData GetNpc(int vnum) => _npcMonsterCache.Get(vnum);

    public List<IMonsterData> GetNpc(string name) => _npcMonsterByName.Get(name);
}