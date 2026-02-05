using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using Plugin.Database.Entities.PlayersData;
using Plugin.Database.Families;
using WingsAPI.Data.Character;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Enums;
using WingsEmu.Packets.Enums.Character;

namespace Plugin.Database.DAOs
{
    public class CharacterDAO(IGenericAsyncLongRepository<CharacterDTO> repository, IDbContextFactory<GameContext> contextFactory, IMapper<CharacterDTO, DbCharacter> mapper)
        : ICharacterDAO
    {
        public async Task<DeleteResult> DeleteByPrimaryKey(long accountId, byte characterSlot)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                // actually a Character wont be deleted, it just will be disabled for future traces
                DbCharacter dbCharacter = await context.Character.FirstOrDefaultAsync(c => c.AccountId == accountId && c.Slot == characterSlot);

                if (dbCharacter == null)
                {
                    return DeleteResult.Deleted;
                }

                context.Character.Remove(dbCharacter);

                DbFamilyMembership familyCharacter = await context.FamilyCharacter.FirstOrDefaultAsync(s => s.CharacterId == dbCharacter.Id);
                if (familyCharacter != null)
                {
                    context.FamilyCharacter.Remove(familyCharacter);
                }

                await context.SaveChangesAsync();

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Log.Error("DeleteByPrimaryKey", e);
                return DeleteResult.Error;
            }
        }

        /// <summary>
        ///     Returns first 30 occurences of highest Compliment
        /// </summary>
        /// <returns></returns>
        public async Task<List<CharacterDTO>> GetTopCompliment(int top = 30)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(c => c.AccountEntity.Authority <= AuthorityType.GS).OrderByDescending(c => c.Compliment).Take(top).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error("GetTopCompliment", e);
                return [];
            }
        }

        /// <summary>
        ///     Returns first 30 occurences of highest Act4Points
        /// </summary>
        /// <returns></returns>
        public async Task<List<CharacterDTO>> GetTopPoints(int top = 30)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(c => c.AccountEntity.Authority <= AuthorityType.GS).OrderByDescending(c => c.Act4Points).Take(top).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error("GetTopPoints", e);
                return [];
            }
        }

        /// <summary>
        ///     Returns first 100 occurences of highest Reputation
        /// </summary>
        /// <returns></returns>
        public async Task<List<CharacterDTO>> GetTopReputation(int top = 100)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(c => c.AccountEntity.Authority <= AuthorityType.GS).OrderByDescending(c => c.Reput).Take(top).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error("GetTopPoints", e);
                return [];
            }
        }


        public IEnumerable<CharacterDTO> LoadByAccount(long accountId)
        {
            try
            {
                using GameContext context = contextFactory.CreateDbContext();
                var characters = context.Character.Where(c => c.AccountId == accountId).OrderByDescending(c => c.Slot).ToList();
                return mapper.Map(characters);
            }
            catch (Exception e)
            {
                Log.Error("LoadByAccount", e);
                return null;
            }
        }

        public async Task<IEnumerable<CharacterDTO>> LoadByAccountAsync(long accountId)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                List<DbCharacter> characters = await context.Character.Where(c => c.AccountId == accountId).OrderByDescending(c => c.Slot).ToListAsync();
                return mapper.Map(characters);
            }
            catch (Exception e)
            {
                Log.Error("LoadByAccount", e);
                return null;
            }
        }

        public IEnumerable<CharacterDTO> LoadAllCharactersByAccount(long accountId)
        {
            try
            {
                using GameContext context = contextFactory.CreateDbContext();
                return mapper.Map(context.Character.Where(c => c.AccountId.Equals(accountId)).OrderByDescending(c => c.Slot).ToList());
            }
            catch (Exception e)
            {
                Log.Error($"LoadAllCharactersByAccount - AccountId: {accountId}", e);
                return [];
            }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAllCharactersByMasterAccountIdAsync(Guid accountId)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(c => c.AccountEntity.MasterAccountId == accountId).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error($"GetAllCharactersByMasterAccountIdAsync - AccountId: {accountId}", e);
                return [];
            }
        }

        public CharacterDTO GetById(long characterId)
        {
            try
            {
                using GameContext context = contextFactory.CreateDbContext();
                return mapper.Map(context.Character.FirstOrDefault(s => s.Id == characterId));
            }
            catch (Exception e)
            {
                Log.Error($"LoadById - CharacterId: {characterId}", e);
                return null;
            }
        }

        public async Task<CharacterDTO> LoadByNameAsync(string name)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                DbCharacter character = await context.Character.FirstOrDefaultAsync(c => c.DeletedAt == null && EF.Functions.ILike(c.Name, name));
                if (character == null)
                {
                    return null;
                }

                return mapper.Map(character);
            }
            catch (Exception e)
            {
                Log.Error($"LoadByNameAsync - Character name: {name}", e);
                throw;
            }
        }

        public CharacterDTO LoadBySlot(long accountId, byte slot)
        {
            try
            {
                using GameContext context = contextFactory.CreateDbContext();
                return mapper.Map(context.Character.FirstOrDefault(c => c.AccountId == accountId && c.Slot == slot));
            }
            catch (Exception e)
            {
                Log.Error($"LoadBySlot - AccountId: {accountId}, slot: {slot}", e);
                return null;
            }
        }

        public async Task<CharacterDTO> LoadBySlotAsync(long accountId, byte slot)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.FirstOrDefaultAsync(c => c.AccountId == accountId && c.Slot == slot));
            }
            catch (Exception e)
            {
                Log.Error($"LoadBySlot - AccountId: {accountId}, slot: {slot}", e);
                return null;
            }
        }

        public async Task<List<CharacterDTO>> GetTopByLevelAsync(int number)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(x => x.AccountEntity.Authority <= AuthorityType.GS).OrderByDescending(s => s.Level).Take(number).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error("GetTopByLevel", e);
                return null;
            }
        }

        public async Task<List<CharacterDTO>> GetTopLevelByClassTypeAsync(ClassType classType, int number)
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return mapper.Map(await context.Character.Where(c => c.AccountEntity.Authority <= AuthorityType.GS && c.Class == classType).OrderByDescending(s => s.Level).Take(number)
                    .ToListAsync());
            }
            catch (Exception e)
            {
                Log.Error("GetTopLevelByClassType", e);
                return [];
            }
        }

        public async Task<Dictionary<ClassType, int>> GetClassesCountAsync()
        {
            try
            {
                await using GameContext context = await contextFactory.CreateDbContextAsync();
                return new Dictionary<ClassType, int>
                {
                    { ClassType.Adventurer, await context.Character.CountAsync(x => x.Class == ClassType.Adventurer) },
                    { ClassType.Archer, await context.Character.CountAsync(x => x.Class == ClassType.Archer) },
                    { ClassType.Magician, await context.Character.CountAsync(x => x.Class == ClassType.Magician) },
                    { ClassType.Swordman, await context.Character.CountAsync(x => x.Class == ClassType.Swordman) },
                    { ClassType.MartialArtist, await context.Character.CountAsync(x => x.Class == ClassType.MartialArtist) }
                };
            }
            catch (Exception e)
            {
                Log.Error("GetClassesCount", e);
                return new Dictionary<ClassType, int>();
            }
        }

        public async Task<IEnumerable<CharacterDTO>> GetAllAsync() => await repository.GetAllAsync();

        public async Task<CharacterDTO> GetByIdAsync(long id) => await repository.GetByIdAsync(id);

        public async Task<IEnumerable<CharacterDTO>> GetByIdsAsync(IEnumerable<long> ids) => await repository.GetByIdsAsync(ids);

        public async Task<CharacterDTO> SaveAsync(CharacterDTO obj) => await repository.SaveAsync(obj);

        public async Task<IEnumerable<CharacterDTO>> SaveAsync(IReadOnlyList<CharacterDTO> objs) => await repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await repository.DeleteByIdsAsync(ids);
        }
    }
}