// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsEmu.DTOs.Relations;

public interface ICharacterRelationDAO
{
    Task<CharacterRelationDTO> GetRelationByCharacterIdAsync(long characterId, long targetId);
    Task SaveRelationsByCharacterIdAsync(long characterId, CharacterRelationDTO relations);
    Task<List<CharacterRelationDTO>> LoadRelationsByCharacterIdAsync(long characterId);
    Task RemoveRelationAsync(CharacterRelationDTO relation);
}