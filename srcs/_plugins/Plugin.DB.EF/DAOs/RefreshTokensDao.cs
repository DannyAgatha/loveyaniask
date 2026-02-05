using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using WingsAPI.Data.Account;

namespace Plugin.Database.DAOs;

public class RefreshTokensDao : IRefreshTokensDao
{
    private readonly IDbContextFactory<GameContext> _contextFactory;
    private readonly IMapper<RefreshTokensEntity, RefreshTokensDto> _mapper;
    private readonly IGenericAsyncLongRepository<RefreshTokensDto> _repository;

    public RefreshTokensDao(IMapper<RefreshTokensEntity, RefreshTokensDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<RefreshTokensDto> repository)
    {
        _mapper = mapper;
        _contextFactory = contextFactory;
        _repository = repository;
    }

    public async Task<RefreshTokensDto> FindToken(long accountId, string token)
    {
        try
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            RefreshTokensEntity refreshTokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(x => x.AccountEntity.Id == accountId && x.Token == token);
            return _mapper.Map(refreshTokenEntity);
        }
        catch (Exception e)
        {
            Log.Error($"FindRefreshToken - AccountId: {accountId}", e);
            return null;
        }
    
    }

    public async Task<IEnumerable<RefreshTokensDto>> GetAccountTokens(long accountId)
    {
        try
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            List<RefreshTokensEntity> refreshTokenEntity = await context.RefreshTokens.Where(x => x.AccountEntity.Id == accountId).ToListAsync();
            return _mapper.Map(refreshTokenEntity);
        }
        catch (Exception e)
        {
            Log.Error($"GetRefreshTokens - AccountId: {accountId}", e);
            return null;
        }
    }
    
    public async Task<IEnumerable<RefreshTokensDto>> GetAllAsync() => await _repository.GetAllAsync();

    public async Task<RefreshTokensDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<RefreshTokensDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

    public async Task<RefreshTokensDto> SaveAsync(RefreshTokensDto obj) => await _repository.SaveAsync(obj);

    public async Task<IEnumerable<RefreshTokensDto>> SaveAsync(IReadOnlyList<RefreshTokensDto> objs) => await _repository.SaveAsync(objs);

    public async Task DeleteByIdAsync(long id) => await _repository.DeleteByIdAsync(id);

    public async Task DeleteByIdsAsync(IEnumerable<long> ids) => await _repository.DeleteByIdsAsync(ids);

}