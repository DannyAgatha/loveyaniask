using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using Plugin.Database.DB;
using WingsEmu.DTOs.Mails;

namespace Plugin.Database.Mail
{
    public class CharacterNoteDao : ICharacterNoteDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbCharacterNote, CharacterNoteDto> _mapper;
        private readonly IGenericAsyncLongRepository<CharacterNoteDto> _repository;

        public CharacterNoteDao(IMapper<DbCharacterNote, CharacterNoteDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<CharacterNoteDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }


        public async Task<List<CharacterNoteDto>> GetByCharacterIdAsync(long characterId)
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            List<DbCharacterNote> notes = await context.Note.Where(s => (s.ReceiverId == characterId && !s.IsSenderCopy || s.IsSenderCopy && s.SenderId == characterId) && s.DeletedAt == null)
                .ToListAsync();
            return _mapper.Map(notes);
        }

        public async Task<IEnumerable<CharacterNoteDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<CharacterNoteDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<CharacterNoteDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<CharacterNoteDto> SaveAsync(CharacterNoteDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<CharacterNoteDto>> SaveAsync(IReadOnlyList<CharacterNoteDto> objs) => await _repository.SaveAsync(objs);

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