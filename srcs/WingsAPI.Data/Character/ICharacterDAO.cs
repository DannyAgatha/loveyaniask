// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;
using WingsEmu.DTOs.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Data.Character;

public interface ICharacterDAO : IGenericAsyncLongRepository<CharacterDTO>
{
    Task<DeleteResult> DeleteByPrimaryKey(long accountId, byte characterSlot);

    Task<List<CharacterDTO>> GetTopCompliment(int top = 30);

    Task<List<CharacterDTO>> GetTopPoints(int top = 30);

    Task<List<CharacterDTO>> GetTopReputation(int top = 100);

    IEnumerable<CharacterDTO> LoadByAccount(long accountId);

    Task<IEnumerable<CharacterDTO>> LoadByAccountAsync(long accountId);

    CharacterDTO GetById(long characterId);

    Task<CharacterDTO> LoadByNameAsync(string name);

    CharacterDTO LoadBySlot(long accountId, byte slot);

    Task<CharacterDTO> LoadBySlotAsync(long accountId, byte slot);

    IEnumerable<CharacterDTO> LoadAllCharactersByAccount(long accountId);

    Task<IEnumerable<CharacterDTO>> GetAllCharactersByMasterAccountIdAsync(Guid accountId);

    Task<List<CharacterDTO>> GetTopByLevelAsync(int number);

    Task<List<CharacterDTO>> GetTopLevelByClassTypeAsync(ClassType classType, int number);

    Task<Dictionary<ClassType, int>> GetClassesCountAsync();
}