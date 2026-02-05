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
    public class FamilyMembershipDao : IFamilyMembershipDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbFamilyMembership, FamilyMembershipDto> _mapper;
        private readonly IGenericAsyncLongRepository<FamilyMembershipDto> _repository;

        public FamilyMembershipDao(IMapper<DbFamilyMembership, FamilyMembershipDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<FamilyMembershipDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }

        public async Task<FamilyMembershipDto> GetByCharacterIdAsync(long characterId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                DbFamilyMembership dbFamilyMembership = await context.FamilyCharacter.FirstOrDefaultAsync(c => c.CharacterId == characterId);

                return _mapper.Map(dbFamilyMembership);
            }
            catch (Exception e)
            {
                Log.Error("GetByCharacterIdAsync", e);
                return null;
            }
        }

        public async Task<List<FamilyMembershipDto>> GetByFamilyIdAsync(long familyId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<DbFamilyMembership> familyCharacter = await context.FamilyCharacter.Where(fc => fc.FamilyId.Equals(familyId)).ToListAsync();
                return _mapper.Map(familyCharacter);
            }
            catch (Exception e)
            {
                Log.Error("GetByFamilyIdAsync", e);
                return null;
            }
        }

        public async Task<IEnumerable<FamilyMembershipDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<FamilyMembershipDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<FamilyMembershipDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<FamilyMembershipDto> SaveAsync(FamilyMembershipDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<FamilyMembershipDto>> SaveAsync(IReadOnlyList<FamilyMembershipDto> objs) => await _repository.SaveAsync(objs);

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