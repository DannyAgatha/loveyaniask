using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.Families;

namespace FamilyServer.Managers
{
    public class FamilyManager
    {
        private readonly ILongKeyCachedRepository<FamilyDTO> _familyCache;
        private readonly IFamilyDAO _familyDao;
        private readonly HashSet<long> _instantiatedFamilyIds = new();

        public FamilyManager(ILongKeyCachedRepository<FamilyDTO> familyCache, IFamilyDAO familyDao)
        {
            _familyCache = familyCache;
            _familyDao = familyDao;
        }

        public async Task<FamilyDTO> AddFamilyAsync(FamilyDTO familyDto)
        {
            FamilyDTO family = await SaveFamilyAsync(familyDto);
            _familyCache.Set(familyDto.Id, family);
            if (!_instantiatedFamilyIds.Contains(family.Id))
            {
                _instantiatedFamilyIds.Add(family.Id);
            }

            return family;
        }

        public async Task RemoveFamilyByIdAsync(long familyId)
        {
            await _familyDao.DeleteByIdAsync(familyId);
            _familyCache.Remove(familyId);
            _instantiatedFamilyIds.Remove(familyId);
        }

        public async Task<FamilyDTO> GetFamilyByFamilyIdAsync(long familyId)
        {
            FamilyDTO family = await _familyCache.GetOrSetAsync(familyId, async () => await _familyDao.GetByIdAsync(familyId));
            if (family == null)
            {
                Log.Error($"[FAMILY_MANAGER] {familyId} could not be found from the cache/db", new DataException($"{familyId} could not be found from the cache/db"));
                return null;
            }

            if (!_instantiatedFamilyIds.Contains(family.Id))
            {
                _instantiatedFamilyIds.Add(family.Id);
            }

            return family;
        }

        public async Task<FamilyDTO> GetFamilyByNameAsync(string name) => await _familyDao.GetByNameAsync(name);

        public async Task<FamilyDTO> SaveFamilyAsync(FamilyDTO familyDto) => await _familyDao.SaveAsync(familyDto);

        public List<FamilyDTO> GetFamiliesInMemory()
        {
            return _instantiatedFamilyIds.Select(s => _familyCache.Get(s)).ToList();
        }
        
        public async Task<IEnumerable<FamilyDTO>> GetFamiliesAsync() => await _familyDao.GetAllAsync();

        public async Task<long> GetReputationInTotalByIdAsync(long familyId) => await _familyDao.GetReputationInTotalByIdAsync(familyId);
    }
}