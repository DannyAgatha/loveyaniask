using Plugin.Database.DAOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.Character;

namespace DatabaseServer.Managers
{
    public interface ICharacterManager
    {
        public Task<IEnumerable<CharacterDTO>> GetCharactersByAccountId(long accountId);
        public Task<CharacterDTO> GetCharacterBySlot(long accountId, byte slot);
        public Task<CharacterDTO> GetCharacterById(long characterId);
        public Task<CharacterDTO> GetCharacterByName(string name);
        public Task<CharacterDTO> CreateCharacter(CharacterDTO characterDto, bool ignoreSlotCheck);
        public Task AddCharacterToSavingQueue(CharacterDTO characterDto);
        public Task AddCharactersToSavingQueue(IEnumerable<CharacterDTO> characterDtos);
        public Task<bool> DeleteCharacter(CharacterDTO characterDto);
        public Task<int> FlushCharacterSaves();
        public Task<CharacterDTO> RemoveCachedCharacter(string requestCharacterName);
    }
}