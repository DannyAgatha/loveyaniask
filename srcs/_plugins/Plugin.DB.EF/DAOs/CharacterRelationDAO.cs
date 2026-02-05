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
using WingsEmu.DTOs.Relations;

namespace Plugin.Database.DAOs
{
    public class CharacterRelationDAO : ICharacterRelationDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<CharacterRelationEntity, CharacterRelationDTO> _mapper;

        public CharacterRelationDAO(
            IMapper<CharacterRelationEntity, CharacterRelationDTO> mapper,
            IDbContextFactory<GameContext> contextFactory
        )
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        public async Task<CharacterRelationDTO> GetRelationByCharacterIdAsync(long characterId, long targetId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                CharacterRelationEntity entity = await context.Set<CharacterRelationEntity>().FindAsync(characterId, targetId);
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Log.Error("GetRelationByCharacterIdAsync", e);
                throw;
            }
        }

        public async Task SaveRelationsByCharacterIdAsync(long characterId, CharacterRelationDTO relations)
        {
            await using GameContext context = _contextFactory.CreateDbContext();

            try
            {
                CharacterRelationEntity obj = _mapper.Map(relations);
                CharacterRelationEntity entity = await context.Set<CharacterRelationEntity>().FindAsync(obj.CharacterId, obj.RelatedCharacterId);

                if (entity == null)
                {
                    entity = obj;
                    entity = (await context.Set<CharacterRelationEntity>().AddAsync(entity)).Entity;
                }
                else
                {
                    context.Entry(entity).CurrentValues.SetValues(obj);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("SaveRelationsByCharacterIdAsync", e);
                throw;
            }
        }

        public async Task<List<CharacterRelationDTO>> LoadRelationsByCharacterIdAsync(long characterId)
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            var relationsToReturn = new List<CharacterRelationDTO>();

            try
            {
                List<CharacterRelationEntity> relations = await context.CharacterRelation.Where(x => x.CharacterId == characterId).ToListAsync();
                foreach (CharacterRelationEntity relation in relations)
                {
                    relationsToReturn.Add(_mapper.Map(relation));
                }

                return relationsToReturn;
            }
            catch (Exception e)
            {
                Log.Error("LoadRelationsByCharacterIdAsync", e);
                throw;
            }
        }

        public async Task RemoveRelationAsync(CharacterRelationDTO relation)
        {
            await using GameContext context = _contextFactory.CreateDbContext();

            try
            {
                CharacterRelationEntity relationToRemove = await context.CharacterRelation.FirstOrDefaultAsync(x => x.CharacterId == relation.CharacterId
                    && x.RelatedCharacterId == relation.RelatedCharacterId && x.RelationType == relation.RelationType);

                if (relationToRemove == null)
                {
                    return;
                }

                context.Remove(relationToRemove);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("RemoveRelationAsync", e);
                throw;
            }
        }
    }
}