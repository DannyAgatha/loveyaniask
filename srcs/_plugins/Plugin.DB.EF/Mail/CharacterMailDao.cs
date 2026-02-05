// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using Plugin.Database.DB;
using WingsEmu.DTOs.Mails;

namespace Plugin.Database.Mail
{
    public class CharacterMailDao : ICharacterMailDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbCharacterMail, CharacterMailDto> _mapper;
        private readonly IGenericAsyncLongRepository<CharacterMailDto> _repository;

        public CharacterMailDao(IMapper<DbCharacterMail, CharacterMailDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<CharacterMailDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }


        public async Task<List<CharacterMailDto>> GetByCharacterIdAsync(long characterId)
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            List<DbCharacterMail> mails = await context.Mail.Where(s => s.ReceiverId == characterId).ToListAsync();
            return _mapper.Map(mails);
        }

        public async Task<IEnumerable<CharacterMailDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<CharacterMailDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<CharacterMailDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<CharacterMailDto> SaveAsync(CharacterMailDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<CharacterMailDto>> SaveAsync(IReadOnlyList<CharacterMailDto> objs) => await _repository.SaveAsync(objs);

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