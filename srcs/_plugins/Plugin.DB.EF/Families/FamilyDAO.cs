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
using Plugin.Database.Entities.PlayersData;
using WingsAPI.Data.Families;

namespace Plugin.Database.Families
{
    public class FamilyDAO : IFamilyDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbFamily, FamilyDTO> _mapper;
        private readonly IGenericAsyncLongRepository<FamilyDTO> _repository;

        public FamilyDAO(IMapper<DbFamily, FamilyDTO> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<FamilyDTO> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }

        public async Task<FamilyDTO> GetByNameAsync(string reqName)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                return _mapper.Map(await context.Family.FirstOrDefaultAsync(s => EF.Functions.ILike(s.Name, $"%{reqName}%")));
            }
            catch (Exception e)
            {
                Log.Error("GetByNameAsync", e);
                throw;
            }
        }

        public async Task<IEnumerable<FamilyDTO>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<FamilyDTO> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<FamilyDTO>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<FamilyDTO> SaveAsync(FamilyDTO obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<FamilyDTO>> SaveAsync(IReadOnlyList<FamilyDTO> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _repository.DeleteByIdsAsync(ids);
        }

        public async Task<long> GetReputationInTotalByIdAsync(long famId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                var player = context.FamilyCharacter.Where(s => s.FamilyId == famId).ToList();
                long reput = 0;
                foreach (DbFamilyMembership p in player)
                {
                    DbCharacter dto = context.Character.FirstOrDefault(s => s.Id == p.CharacterId);
                    if (dto == null) continue;
                    reput += dto.Reput;
                }
                return reput;
            }
            catch (Exception e)
            {
                Log.Error("GetReputationInTotalByIdAsync", e);
                throw;
            }
        }
    }
}