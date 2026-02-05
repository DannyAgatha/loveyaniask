// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using WingsAPI.Data.Families;

namespace Plugin.Database.Families
{
    public class FamilyLogDAO : IFamilyLogDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbFamilyLog, FamilyLogDto> _mapper;
        private readonly IGenericAsyncLongRepository<FamilyLogDto> _repository;

        public FamilyLogDAO(IMapper<DbFamilyLog, FamilyLogDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<FamilyLogDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }


        public async Task<List<FamilyLogDto>> GetLogsByFamilyIdAsync(long familyId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<DbFamilyLog> tmp = await context.FamilyLog.Where(s => s.FamilyId.Equals(familyId)).OrderByDescending(s => s.Timestamp).Take(200).ToListAsync();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                Log.Error("LoadById", e);
                return null;
            }
        }

        public async Task<IEnumerable<FamilyLogDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<FamilyLogDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<FamilyLogDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<FamilyLogDto> SaveAsync(FamilyLogDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<FamilyLogDto>> SaveAsync(IReadOnlyList<FamilyLogDto> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _repository.DeleteByIdsAsync(ids);
        }
    }
}