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
using WingsAPI.Data.Bazaar;
using WingsEmu.DTOs.Bazaar;

namespace Plugin.Database.Bazaar
{
    public class BazaarItemDAO : IBazaarItemDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbBazaarItemEntity, BazaarItemDTO> _mapper;
        private readonly IGenericAsyncLongRepository<BazaarItemDTO> _repository;

        public BazaarItemDAO(IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<BazaarItemDTO> repository, IMapper<DbBazaarItemEntity, BazaarItemDTO> mapper)
        {
            _contextFactory = contextFactory;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BazaarItemDTO>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<BazaarItemDTO> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<BazaarItemDTO>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<BazaarItemDTO> SaveAsync(BazaarItemDTO obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<BazaarItemDTO>> SaveAsync(IReadOnlyList<BazaarItemDTO> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _repository.DeleteByIdsAsync(ids);
        }

        public async Task<IReadOnlyCollection<BazaarItemDTO>> GetAllNonDeletedBazaarItems()
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                var tmp = context.BazaarItem.Where(s => s.DeletedAt == null).ToList();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                Log.Error("GetBazaarItemByItemInstanceId", e);
                return null;
            }
        }

        public async Task<IReadOnlyList<BazaarItemDTO>> GetBazaarItemsByCharacterId(long characterId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                var list = new List<BazaarItemDTO>();
                IQueryable<DbBazaarItemEntity> tmp = context.BazaarItem.Where(s => s.CharacterId == characterId);
                foreach (DbBazaarItemEntity item in tmp)
                {
                    list.Add(_mapper.Map(item));
                }

                return list.AsReadOnly();
            }
            catch (Exception e)
            {
                Log.Error("GetBazaarItemsByCharacterId", e);
                return null;
            }
        }
    }
}