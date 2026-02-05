using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoenixLib.DAL.EFCore.PGSQL
{
    /// <summary>
    ///     GenericAsyncMappedRepository is an asynchronous Data Access Object
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public sealed class GenericMappedLongRepository<TEntity, TDto> : IGenericAsyncLongRepository<TDto>
    where TDto : class, ILongDto, new()
    where TEntity : class, ILongEntity, new()
    {
        private readonly IMapper<TEntity, TDto> _mapper;
        private readonly IGenericAsyncLongRepository<TEntity> _repository;

        public GenericMappedLongRepository(IMapper<TEntity, TDto> mapper, IGenericAsyncLongRepository<TEntity> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<IEnumerable<TDto>> GetAllAsync() => _mapper.Map(await _repository.GetAllAsync());

        public async Task<TDto> GetByIdAsync(long id) => _mapper.Map(await _repository.GetByIdAsync(id));

        public async Task<IEnumerable<TDto>> GetByIdsAsync(IEnumerable<long> ids) => _mapper.Map(await _repository.GetByIdsAsync(ids));

        public async Task<TDto> SaveAsync(TDto obj) => _mapper.Map(await _repository.SaveAsync(_mapper.Map(obj)));

        public async Task<IEnumerable<TDto>> SaveAsync(IReadOnlyList<TDto> objs) => _mapper.Map(await _repository.SaveAsync(_mapper.Map(objs)));

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