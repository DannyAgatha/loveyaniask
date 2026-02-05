using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Mails;

public interface ICharacterNoteDao : IGenericAsyncLongRepository<CharacterNoteDto>
{
    Task<List<CharacterNoteDto>> GetByCharacterIdAsync(long characterId);
}