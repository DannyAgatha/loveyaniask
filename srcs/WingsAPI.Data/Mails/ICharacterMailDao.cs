// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Mails;

public interface ICharacterMailDao : IGenericAsyncLongRepository<CharacterMailDto>
{
    Task<List<CharacterMailDto>> GetByCharacterIdAsync(long characterId);
}