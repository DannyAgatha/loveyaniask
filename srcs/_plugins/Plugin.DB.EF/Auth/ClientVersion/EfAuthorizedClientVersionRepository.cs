using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoenixLib.DAL;
using Plugin.Database.Auth.HWID;
using Plugin.Database.DB;
using WingsAPI.Communication.Auth;

namespace Plugin.Database.Auth.ClientVersion
{
    public class EfAuthorizedClientVersionRepository : IAuthorizedClientVersionRepository
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly ILogger<BlacklistedHwidDao> _logger;
        private readonly IMapper<AuthorizedClientVersionDto, AuthorizedClientVersionEntity> _mapper;
        private readonly IGenericAsyncLongRepository<AuthorizedClientVersionDto> _repository;

        public EfAuthorizedClientVersionRepository(IGenericAsyncLongRepository<AuthorizedClientVersionDto> repository, IDbContextFactory<GameContext> contextFactory,
            ILogger<BlacklistedHwidDao> logger, IMapper<AuthorizedClientVersionDto, AuthorizedClientVersionEntity> mapper)
        {
            _repository = repository;
            _contextFactory = contextFactory;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<AuthorizedClientVersionDto> DeleteAsync(string clientVersion)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                AuthorizedClientVersionEntity entity = await context.AuthorizedClientVersions.FirstOrDefaultAsync(s => s.ClientVersion == clientVersion);
                if (entity == null)
                {
                    return null;
                }

                context.AuthorizedClientVersions.Remove(entity);
                await context.SaveChangesAsync();
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                _logger.LogError("DeleteAsync", e);
                throw;
            }
        }

        public async Task<IEnumerable<AuthorizedClientVersionDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<AuthorizedClientVersionDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<AuthorizedClientVersionDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<AuthorizedClientVersionDto> SaveAsync(AuthorizedClientVersionDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<AuthorizedClientVersionDto>> SaveAsync(IReadOnlyList<AuthorizedClientVersionDto> objs) => await _repository.SaveAsync(objs);

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