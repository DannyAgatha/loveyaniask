// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;
using WingsAPI.Data.Bazaar;

namespace WingsEmu.DTOs.Bazaar;

public interface IBazaarItemDAO : IGenericAsyncLongRepository<BazaarItemDTO>
{
    Task<IReadOnlyCollection<BazaarItemDTO>> GetAllNonDeletedBazaarItems();
    Task<IReadOnlyList<BazaarItemDTO>> GetBazaarItemsByCharacterId(long characterId);
}