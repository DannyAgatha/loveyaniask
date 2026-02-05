using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;

namespace WingsEmu.Game.Bazaar;

public interface IBazaarManager
{
    Task<string> GetOwnerName(long characterId);

    Task<BazaarItem> GetBazaarItemById(long bazaarItemId);

    Task<IReadOnlyCollection<BazaarItem>> GetListedItemsByCharacterId(long characterId);

    Task<(IReadOnlyCollection<BazaarItem>, RpcResponseType)> SearchBazaarItems(BazaarSearchContext bazaarSearchContext);
}